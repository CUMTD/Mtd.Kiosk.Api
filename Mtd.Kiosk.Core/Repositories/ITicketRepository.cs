using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Core.Repositories
{
	public interface ITicketRepository : IRepository<Ticket>
	{
		Task<List<Ticket>> GetByKioskIdAsync(string kioskId, CancellationToken cancellationToken);

		Task<IReadOnlyCollection<Ticket>> GetAllOpenTicketsAsync(CancellationToken cancellationToken);

		Task<int> GetOpenTicketCountAsync(string kioskId, CancellationToken cancellationToken);


	}
}
