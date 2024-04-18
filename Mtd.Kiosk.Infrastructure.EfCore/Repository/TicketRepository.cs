using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using System.Collections.Immutable;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository
{
	public class TicketRepository(KioskContext context) : AsyncEFIdentifiableRepository<string, Ticket>(context), ITicketRepository
	{
		public async Task<IReadOnlyCollection<Ticket>> GetAllOpenTicketsAsync(CancellationToken cancellationToken)
		{
			var openTickets = await _dbSet.Where(t => t.Status != TicketStatusType.Resolved).ToArrayAsync(cancellationToken);
			return openTickets.ToImmutableArray();
		}

		Task<List<Ticket>> ITicketRepository.GetByKioskIdAsync(string kioskId, CancellationToken cancellationToken)
		{
			return _dbSet
				.Where(t => t.KioskId == kioskId)
				.ToListAsync(cancellationToken);

		}
	}
}
