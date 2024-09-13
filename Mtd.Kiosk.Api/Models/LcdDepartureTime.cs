using Mtd.Kiosk.RealTime.Entities;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// A single departure time for the LCD.
/// </summary>
/// <remarks>
/// Constructor that maps a generic departure to an LCD departure time.
/// </remarks>
/// <param name="departure">The departure time to use.</param>
public class LcdDepartureTime(Departure departure)
{
	private static readonly string[] _nonDestinationStrings = ["express"];

	/// <summary>
	/// The time of the departure. Can be "DUE", "1 min", "XX mins", or "HH:MM AM".
	/// </summary>
	[JsonPropertyName("time")]
	public string Time { get; set; } = departure.DepartsIn;

	/// <summary>
	/// Whether the departure is real-time or not.
	/// </summary>
	[JsonPropertyName("isRealTime")]
	public bool IsRealTime { get; set; } = departure.IsRealTime;

	/// <summary>
	/// Whether the departure is a hopper or not.
	/// </summary>
	[JsonPropertyName("isHopper")]
	public bool IsHopper { get; set; } = departure.IsHopper;

	/// <summary>
	/// The modifier text for the departure, e.g. "Hopper" or "To Gerty".
	/// </summary>
	[JsonPropertyName("modifier")]
	public string Modifier
	{
		get
		{

			var routeIdWithoutHopper = Regex.Replace(departure.RouteId, "hopper", string.Empty, RegexOptions.IgnoreCase);
			var destination = Regex.Replace(departure.RouteColor, $"{routeIdWithoutHopper}|hopper", string.Empty, RegexOptions.IgnoreCase).Trim();

			if (destination.Length > 0)
			{
				if (_nonDestinationStrings.Any(str => string.Equals(str, destination, StringComparison.OrdinalIgnoreCase)))
				{
					return destination;
				}

				return $"to {destination}";
			}

			return string.Empty;

		}
		set
		{
			// Do nothing
		}
	}
}

