using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository
{
	public class KioskRepository(KioskContext context) : AsyncEFIdentifiableRepository<string, Core.Entities.Kiosk>(context), IKioskRepository
	{
		public Task<Core.Entities.Kiosk> GetByIdentityWithTicketsAsync(string identity, CancellationToken cancellationToken)
		{
			return _dbSet.Include(k => k.Tickets)
				.SingleAsync(k => k.Id == identity, cancellationToken);
		}

		Task<List<Core.Entities.Kiosk>> IKioskRepository.GetAllAsync(CancellationToken cancellationToken)
		{
			return _dbSet.ToListAsync(cancellationToken);
		}
	}
}
