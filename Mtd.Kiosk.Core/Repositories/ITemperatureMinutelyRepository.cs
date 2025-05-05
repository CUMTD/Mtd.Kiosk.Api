using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Core.Repositories;

public interface ITemperatureMinutelyRepository : IRepository<TemperatureMinutely>
{

	Task<IReadOnlyCollection<TemperatureMinutely>> GetPastMonthTempsAsync(string kioskId, CancellationToken cancellationToken);

	Task<IReadOnlyCollection<TemperatureMinutely>> GetDayDataAsync(string kioskId, DateTime date, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<string>> GetTemperatureLoggingKioskIds(CancellationToken cancellationToken);

	Task<TemperatureDaily?> AggregateDayData(string kioskId, DateTime date, CancellationToken cancellationToken);

	Task<IReadOnlyCollection<DateTime>> GetUniqueDaysWithDataByKioskId(string kioskId, CancellationToken cancellationToken);

	void DeleteDataByDate(DateTime date, CancellationToken cancellationToken);

}
