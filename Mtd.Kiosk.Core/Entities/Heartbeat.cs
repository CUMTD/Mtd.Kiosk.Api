using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities;

public class Heartbeat : GuidEntity, IEntity
{
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;
	public string KioskId { get; set; }
	public HeartbeatType Type { get; set; }

	[SetsRequiredMembers]
	protected Heartbeat() : base()
	{
		KioskId = string.Empty;
		Timestamp = DateTime.UtcNow;
	}

	[SetsRequiredMembers]
	public Heartbeat(string kioskId, HeartbeatType type) : this()
	{
		KioskId = kioskId;
		Type = type;
	}
}
