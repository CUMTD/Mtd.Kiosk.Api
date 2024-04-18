using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository
{
	public class HeartbeatRepository(KioskContext context) : AsyncEFIdentifiableRepository<string, Mtd.Kiosk.Core.Entities.Heartbeat>(context), IHeartbeatRepository
	{


		Task<List<Heartbeat>> IHeartbeatRepository.GetByIdentityAndHeartbeatTypeAsync(string identity, HeartbeatType heartbeatType, CancellationToken cancellationToken)
		{
			return _dbSet
				.Where(h => h.KioskId == identity && h.Type == heartbeatType)
				.ToListAsync(cancellationToken);
		}
	}
}
