using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities;

public class Heartbeat : GuidEntity, IEntity
{
	public required DateTime Timestamp { get; set; } = DateTime.UtcNow;
	public required string KioskId { get; set; }
	public required HeartbeatType Type { get; set; }

	[SetsRequiredMembers]
	protected Heartbeat() : base()
	{
		Timestamp = DateTime.UtcNow;
	}

	[SetsRequiredMembers]
	public Heartbeat(string kioskId, HeartbeatType type) : this()
	{
		KioskId = kioskId;
		Type = type;
	}
}
