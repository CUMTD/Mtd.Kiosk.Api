using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for creating a new heartbeat.
/// </summary>
public class NewHeartbeatModel
{
	/// <summary>
	/// The id of the kiosk to log the heartbeat for.
	/// </summary>
	public required string KioskId { get; set; }
	/// <summary>
	/// The type of heartbeat to log.
	/// </summary>
	public required HeartbeatType Type { get; set; }
	/// <summary>
	/// Converts the model to a Heartbeat.
	/// </summary>
	/// <returns></returns>
	public Heartbeat ToHeartbeat() => new(KioskId, Type);
	/// <summary>
	/// Constructor for NewHeartbeatModel.
	/// </summary>
	/// <param name="kioskId"></param>
	/// <param name="type"></param>
	public NewHeartbeatModel(string kioskId, HeartbeatType type)
	{
		KioskId = kioskId;
		Type = type;
	}
}
