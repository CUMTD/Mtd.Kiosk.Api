using Mtd.Core.Repositories;
using Mtd.Core.Entities;

namespace Mtd.Kiosk.Core.Repositories
{
	public interface IRepository<T> :
    IAsyncIdentifiable<string, T>,
    IAsyncReadable<T, IReadOnlyCollection<T>>,
    IAsyncWriteable<T, IReadOnlyCollection<T>>
    where T : class, IIdentity<string>
    {
    }
}
