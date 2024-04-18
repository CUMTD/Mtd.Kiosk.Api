using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Core.Repositories
{
	public interface IHeartbeatRepository : IRepository<Heartbeat>
	{

		Task<List<Heartbeat>> GetByIdentityAndHeartbeatTypeAsync(string identity, HeartbeatType heartbeatType, CancellationToken cancellationToken);

	}
}
