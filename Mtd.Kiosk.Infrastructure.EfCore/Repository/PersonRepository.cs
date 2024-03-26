using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using System.Collections.Immutable;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository
{
	public class PersonRepository(KioskContext context) : AsyncEFIdentifiableRepository<string, Person>(context), IPersonRepository<IReadOnlyCollection<Person>>
	{
		public async Task<IReadOnlyCollection<Person>> GetByFirstNameAsync(string first, CancellationToken cancellationToken)
		{
			var result = await _dbSet
				.AsQueryable()
				.Where(p => p.First == first)
				.ToArrayAsync(cancellationToken);

			return result
				.ToImmutableArray();
		}
	}
}
