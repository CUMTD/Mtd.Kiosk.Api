using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Core.Repositories;

public interface ITemperatureRepository : IRepository<Temperature>
{

	Task<IReadOnlyCollection<Temperature>> GetPastMonthTempsAsync(string kioskId, CancellationToken cancellationToken);

}
