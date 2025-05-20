using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Services;

/// <summary>
/// Background service that runs every 24 hours. Checks if there is a full previous day of data, and if there is, calculates some aggregated data points and deletes the data.
/// </summary>
public class ScopedTemperatureAggregatorWorker
{
	private readonly ITemperatureMinutelyRepository _temperatureRepository;
	private readonly ITemperatureDailyRepository _temperatureDailyRepository;
	private readonly ILogger<TemperatureAggregatorWorker> _logger;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="temperatureRepository"></param>
	/// <param name="temperatureDailyRepository"></param>
	/// <param name="logger"></param>
	public ScopedTemperatureAggregatorWorker(ITemperatureMinutelyRepository temperatureRepository, ITemperatureDailyRepository temperatureDailyRepository, ILogger<TemperatureAggregatorWorker> logger)
	{
		_temperatureRepository = temperatureRepository;
		_temperatureDailyRepository = temperatureDailyRepository;
		_logger = logger;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="cancellationToken"></param>
	internal async Task DoWorkAsync(CancellationToken cancellationToken)
	{
		// number of days to keep minuely data 122
		//var minutelyDataRetentionDays = 122;

		//get all kiosk Ids that are logging temperature data (move this to sanity?)
		var tempLoggingKiosks = await _temperatureRepository.GetTemperatureLoggingKioskIds(cancellationToken);

		if (tempLoggingKiosks == null || tempLoggingKiosks.Count == 0)
		{
			_logger.LogWarning("No minutely temp data found.");
			return;
		}

		// for each kiosk id that's logging temperature...
		foreach (var kioskId in tempLoggingKiosks)
		{
			// get the unique days that have data
			var uniqueDays = await _temperatureRepository.GetUniqueDaysWithDataByKioskId(kioskId, cancellationToken);

			// drop today's date
			uniqueDays = uniqueDays.Where(d => d.Date != DateTime.Now.Date).ToArray();

			// for each day with data...
			foreach (var day in uniqueDays)
			{

				// if data has already been aggregated, delete the minutely data
				if (await _temperatureDailyRepository.AnyAsync(td => td.Date != day, cancellationToken))
				{
					var aggregatedTempData = await _temperatureRepository.AggregateDayData(kioskId, day, cancellationToken);
					if (aggregatedTempData != null)
					{
						await _temperatureDailyRepository.AddAsync(aggregatedTempData, cancellationToken);

					}
					else
					{
						_logger.LogWarning("Data aggregation returned null.");
					}
				}
				//}
			}
		}

		await _temperatureDailyRepository.CommitChangesAsync(cancellationToken);
	}
}
