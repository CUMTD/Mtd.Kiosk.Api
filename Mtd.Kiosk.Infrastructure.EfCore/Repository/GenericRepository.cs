using Microsoft.EntityFrameworkCore;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository
{
	public abstract class GenericRepository<T_Entity>(KioskContext context) where T_Entity : class
	{

		protected readonly KioskContext _context = context;
		protected readonly DbSet<T_Entity> _dbSet = context.Set<T_Entity>();

		public void Delete(T_Entity entity)
		{
			_dbSet.Remove(entity);

		}
	}
}
