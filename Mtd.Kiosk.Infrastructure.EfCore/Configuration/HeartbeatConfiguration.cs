using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Infrastructure.EfCore.Configuration
{
	internal class HeartbeatConfiguration : IEntityTypeConfiguration<Heartbeat>
	{
		public void Configure(EntityTypeBuilder<Heartbeat> builder)
		{
			_ = builder.ToTable("Heartbeat");

			_ = builder.HasKey(k => k.Id);

			_ = builder.Property(k => k.KioskId)
				.IsRequired();

			_ = builder.Property(k => k.Timestamp)
				.IsRequired();

			_ = builder.Property(k => k.Type)
				.IsRequired();
		}
	}
}
