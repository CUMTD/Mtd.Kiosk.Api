using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository;

public class TemperatureRepository(KioskContext context) : AsyncEFRepository<Temperature>(context), ITemperatureRepository
{
	public async Task<IReadOnlyCollection<Temperature>> GetPastMonthTempsAsync(string kioskId, CancellationToken cancellationToken)
	{

		var temps = await _dbSet.Where(t => t.Timestamp >= DateTimeOffset.UtcNow.AddDays(-30) && t.KioskId == kioskId)
			.OrderBy(t => t.Timestamp)
			.ToArrayAsync(cancellationToken);

		return temps;

	}
}
