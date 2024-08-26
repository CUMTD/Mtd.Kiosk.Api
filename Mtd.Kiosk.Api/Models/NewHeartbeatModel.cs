﻿using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for creating a new heartbeat.
/// </summary>
/// <remarks>
/// Constructor for NewHeartbeatModel.
/// </remarks>
/// <param name="kioskId"></param>
/// <param name="type"></param>
public class NewHeartbeatModel(string kioskId, HeartbeatType type)
{
	/// <summary>
	/// The id of the kiosk to log the heartbeat for.
	/// </summary>
	public required string KioskId { get; set; } = kioskId;
	/// <summary>
	/// The type of heartbeat to log.
	/// </summary>
	public required HeartbeatType Type { get; set; } = type;
	/// <summary>
	/// Converts the model to a Heartbeat.
	/// </summary>
	/// <returns></returns>
	public Heartbeat ToHeartbeat() => new(KioskId, Type);
}
