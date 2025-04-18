using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Infrastructure.EfCore.Configuration;

internal class TemperatureConfiguration : IEntityTypeConfiguration<Temperature>
{
	public void Configure(EntityTypeBuilder<Temperature> builder)
	{
		_ = builder
			.ToTable("Temperature");

		_ = builder
			.HasKey(t => new { t.KioskId, t.Timestamp });

		_ = builder
			.Property(t => t.TempFahrenheit)
			.IsRequired();

		_ = builder
			.Property(t => t.RelHumidity)
			.IsRequired();

	}
}
