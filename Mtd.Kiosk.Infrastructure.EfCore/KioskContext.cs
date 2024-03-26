using Microsoft.EntityFrameworkCore;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Infrastructure.EfCore.Configuration;

namespace Mtd.Kiosk.Infrastructure.EfCore
{
	public class KioskContext(DbContextOptions<KioskContext> options) : DbContext(options)
	{
		public DbSet<Person> Persons { get; protected set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfiguration(new PersonConfiguration());
		}
	}
}
