using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Infrastructure.EfCore.Configuration
{
	internal class PersonConfiguration : IEntityTypeConfiguration<Person>
	{
		public void Configure(EntityTypeBuilder<Person> builder)
		{
			_ = builder
				.ToTable("Person");

			_ = builder
				.HasKey(p => p.Id);

			_ = builder
				.Property(p => p.First)
				.HasMaxLength(100)
				.IsRequired();

			_ = builder
				.Property(p => p.Last)
				.HasMaxLength(100)
				.IsRequired();
		}
	}
}
