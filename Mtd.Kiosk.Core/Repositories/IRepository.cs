using Mtd.Core.Entities;

namespace Mtd.Kiosk.Core.Repositories
{
	public interface IRepository<T_Entity> where T_Entity : IEntity
	{
		void Delete(T_Entity entity);

	}
}
