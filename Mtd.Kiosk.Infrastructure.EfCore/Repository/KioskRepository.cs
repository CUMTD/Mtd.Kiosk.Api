using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository
{
	public class KioskRepository : GenericRepository<Core.Entities.Kiosk>, IKioskRepository
	{
		public KioskRepository(KioskContext context) : base(context)
		{
		}
	}
}
