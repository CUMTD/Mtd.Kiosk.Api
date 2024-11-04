using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities;

public class Health : IEntity
{
	public string KioskId { get; set; }
	public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
	public HeartbeatType Type { get; set; }
	public virtual Kiosk? Kiosk { get; set; }

	[SetsRequiredMembers]
	protected Health() : base()
	{
		KioskId = string.Empty;
		LastHeartbeat = DateTime.UtcNow;
	}

	[SetsRequiredMembers]
	public Health(string kioskId, HeartbeatType type) : this()
	{
		KioskId = kioskId;
		Type = type;
	}

	public HealthStatus GetHealthStatusForTime(DateTime time, int warningMinutes, int errorMinutes)
	{
		var timeSinceLastHeartbeat = time - LastHeartbeat;

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
