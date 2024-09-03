using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities;

public class Heartbeat : GuidEntity, IEntity
{
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;
	public string KioskId { get; set; }
	public HeartbeatType Type { get; set; }
	public virtual Kiosk? Kiosk { get; set; }

	[SetsRequiredMembers]
	protected Heartbeat() : base()
	{
		KioskId = string.Empty;
		Timestamp = DateTime.UtcNow;
	}

	[SetsRequiredMembers]
	public Heartbeat(string kioskId, HeartbeatType type, Kiosk kiosk) : this()
	{
		KioskId = kioskId;
		Type = type;
		Kiosk = kiosk;
	}

	public HealthStatus GetHealthStatusForTime(DateTime time, int warningMinutes, int errorMinutes)
	{
		var timeSinceLastHeartbeat = time - Timestamp;

		if (timeSinceLastHeartbeat > TimeSpan.FromMinutes(errorMinutes))
		{
			return HealthStatus.Critical;
		}

		if (timeSinceLastHeartbeat > TimeSpan.FromMinutes(warningMinutes))
		{
			return HealthStatus.Warning;
		}

		return HealthStatus.Healthy;
	}
}
