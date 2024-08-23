using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Api.Models;

public class NewHeartbeatModel
{
	public required string KioskId { get; set; }
	public required HeartbeatType Type { get; set; }

	public Heartbeat ToHeartbeat() => new(KioskId, Type);

	public NewHeartbeatModel(string kioskId, HeartbeatType type)
	{
		KioskId = kioskId;
		Type = type;
	}
}
