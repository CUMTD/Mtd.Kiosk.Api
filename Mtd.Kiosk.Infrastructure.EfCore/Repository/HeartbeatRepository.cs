using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using System.Collections.Immutable;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository;

public class HeartbeatRepository(KioskContext context) : AsyncEFIdentifiableRepository<string, Mtd.Kiosk.Core.Entities.Heartbeat>(context), IHeartbeatRepository
{

	public async Task<IReadOnlyCollection<Heartbeat>> GetByIdentityAndHeartbeatTypeAsync(string identity, HeartbeatType heartbeatType, CancellationToken cancellationToken)
	{
		var heartbeats = await _dbSet
			.Where(h => h.KioskId == identity && h.Type == heartbeatType)
			.ToArrayAsync(cancellationToken);

		return heartbeats.ToImmutableArray();

	}

	public Task<Heartbeat?> GetMostRecentHeartbeatOfTypeOrDefaultAsync(string identity, HeartbeatType heartbeatType, CancellationToken cancellationToken) => _dbSet
		.Where(h => h.KioskId == identity && h.Type == heartbeatType)
		.OrderByDescending(h => h.Timestamp)
		.FirstOrDefaultAsync(cancellationToken);
}
