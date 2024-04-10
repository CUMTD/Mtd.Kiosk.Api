using Mtd.Core.Entities;

namespace Mtd.Kiosk.Core.Entities
{
	public class Heartbeat : GuidEntity, IEntity
	{
		public required DateTime Timestamp { get; set; }
		public required string KioskId { get; set; }
		public required HeartbeatType Type { get; set; }

	}
}
