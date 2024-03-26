using Mtd.Core.Repositories;
using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Core.Repositories
{
	public interface IPersonRepository<T_Collection> : IAsyncReadable<Person, T_Collection>, IAsyncWriteable<Person, T_Collection>, IAsyncIdentifiable<string, Person>, IDisposable
		where T_Collection : IEnumerable<Person>
	{
		Task<T_Collection> GetByFirstNameAsync(string, CancellationToken cancellationToken);
	}
}
