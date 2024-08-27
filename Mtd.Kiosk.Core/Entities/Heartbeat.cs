using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities;

public class Heartbeat : GuidEntity, IEntity
{
	public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
	public string KioskId { get; set; }
	public HeartbeatType Type { get; set; }
	public virtual Kiosk Kiosk { get; set; }

	[SetsRequiredMembers]
	protected Heartbeat() : base()
	{
		KioskId = string.Empty;
		Timestamp = DateTimeOffset.UtcNow;
		Kiosk = new Kiosk();
	}

	[SetsRequiredMembers]
	public Heartbeat(string kioskId, HeartbeatType type) : this()
	{
		KioskId = kioskId;
		Type = type;
	}

	public HealthStatus GetHealthStatusForTime(DateTimeOffset time, int warningMinutes, int errorMinutes)
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
