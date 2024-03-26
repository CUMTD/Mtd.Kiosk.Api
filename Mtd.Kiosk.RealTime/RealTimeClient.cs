using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.RealTime.Config;
using Mtd.Kiosk.RealTime.Entities;
using System.Text.Json;

namespace Mtd.Kiosk.RealTime
{
	public class RealTimeClient
	{
		public readonly RealTimeClientConfig _config;
		public readonly HttpClient _client;
		public readonly ILogger<RealTimeClient> _logger;

		public RealTimeClient(IOptions<RealTimeClientConfig> config, HttpClient client, ILogger<RealTimeClient> logger)
		{
			ArgumentNullException.ThrowIfNull(config?.Value, nameof(config));
			ArgumentNullException.ThrowIfNull(client, nameof(client));
			ArgumentNullException.ThrowIfNull(logger, nameof(logger));

			_config = config.Value;
			_client = client;
			_logger = logger;
		}

		public async Task<StopMonitoringResult?> GetRealTimeForStop(string stopId, CancellationToken cancellationToken)
		{
			HttpResponseMessage? httpResponseMessage;
			try
			{
				httpResponseMessage = await _client.GetAsync($"{_config.SmUri?.ToString()}?stopIds={stopId}&previewTime=60", cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting real-time data for stop {StopId}", stopId);
				return null;
			}

			if (httpResponseMessage == null)
			{
				_logger.LogError("Error getting real-time data for stop {StopId}", stopId);
				return null;
			}

			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				_logger.LogError("Error getting real-time data for stop {StopId}: {StatusCode}", stopId, httpResponseMessage.StatusCode);
			}

			try
			{
				using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
				var smResult = await JsonSerializer.DeserializeAsync<StopMonitoringResult>(contentStream, cancellationToken: cancellationToken);
				return smResult;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deserializing real-time data for stop {StopId}", stopId);
				return null;
			}
		}
	}
}
