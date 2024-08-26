using Mtd.Kiosk.RealTime.Entities;
using Mtd.Stopwatch.Core.Entities.Schedule;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models.Enums;

/// <summary>
/// Model for the response of the LCD departure endpoint.
/// </summary>
public class LcdDepartureResponseModel
{
	/// <summary>
	/// A list of upcoming departures.
	/// </summary>
	[JsonPropertyName("routes")]
	public IReadOnlyCollection<LcdDeparture> Routes { get; set; }

	/// <summary>
	/// Current general message to display, if any.
	/// </summary>
	[JsonPropertyName("generalMessage")]
	public LcdGeneralMessage? GeneralMessage { get; set; }

	/// <summary>
	/// Constructor for the LCD departure response model.
	/// </summary>
	/// <param name="routes"></param>
	/// <param name="generalMessage"></param>
	public LcdDepartureResponseModel(IEnumerable<LcdDeparture> routes, LcdGeneralMessage? generalMessage)
	{
		Routes = routes.ToImmutableArray();
		GeneralMessage = generalMessage;
	}
}

/// <summary>
/// A single departure for the LCD.
/// </summary>
public class LcdDeparture
{
	/// <summary>
	/// The route number.
	/// </summary>
	[JsonPropertyName("number")]
	public string? Number { get; set; }

	/// <summary>
	/// The route name.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; }

	/// <summary>
	/// The direction/headsign of the route.
	/// </summary>
	[JsonPropertyName("direction")]
	public string Direction { get; set; }

	/// <summary>
	/// The foreground hex color of the route, including the #.
	/// </summary>
	[JsonPropertyName("foregroundHexColor")]
	public string ForegroundHexColor { get; set; }
	/// <summary>
	/// The background hex color of the route, including the #.
	/// </summary>
	[JsonPropertyName("backgroundHexColor")]
	public string BackgroundHexColor { get; set; }

	/// <summary>
	/// The sort order of the route.
	/// </summary>
	[JsonPropertyName("sortOrder")]
	public int SortOrder { get; set; }

	/// <summary>
	/// The departure times for the route.
	/// </summary>
	[JsonPropertyName("departureTimes")]
	public List<LcdDepartureTime> DepartureTimes { get; set; }

	/// <summary>
	/// Constructor that maps a generic publicRoute and list of times to an LCD departure.
	/// </summary>
	/// <param name="publicRoute"></param>
	/// <param name="lcdDepartures"></param>
	/// <param name="direction"></param>
	public LcdDeparture(PublicRoute publicRoute, List<LcdDepartureTime> lcdDepartures, string direction)
	{
		Number = publicRoute.RouteNumber;
		Name = publicRoute.PublicRouteGroup.RouteName;
		Direction = direction;
		ForegroundHexColor = $"#{publicRoute.PublicRouteGroup.HexTextColor}";
		BackgroundHexColor = $"#{publicRoute.PublicRouteGroup.HexColor}";
		DepartureTimes = lcdDepartures;
		SortOrder = publicRoute.PublicRouteGroup.SortNumber;
	}
}

/// <summary>
/// A single departure time for the LCD.
/// </summary>
public class LcdDepartureTime
{
	/// <summary>
	/// The time of the departure. Can be "DUE", "1 min", "XX mins", or "HH:MM AM".
	/// </summary>
	[JsonPropertyName("time")]
	public string Time { get; set; }

	/// <summary>
	/// Whether the departure is real-time or not.
	/// </summary>
	[JsonPropertyName("isRealTime")]
	public bool IsRealTime { get; set; }

	/// <summary>
	/// Whether the departure is a hopper or not.
	/// </summary>
	[JsonPropertyName("isHopper")]
	public bool IsHopper { get; set; }

	/// <summary>
	/// Constructor that maps a generic departure to an LCD departure time.
	/// </summary>
	/// <param name="departure"></param>
	public LcdDepartureTime(Departure departure)
	{
		Time = departure.DepartsIn;
		IsRealTime = departure.IsRealTime;
		IsHopper = departure.IsHopper;
	}
}

/// <summary>
/// A general message for the LCD.
/// </summary>
public class LcdGeneralMessage
{
	/// <summary>
	/// Whether the message blocks real-time departures.
	/// </summary>
	[JsonPropertyName("blocksRealtime")]
	public bool BlocksRealTime { get; set; }
	/// <summary>
	/// The text of the message.
	/// </summary>
	[JsonPropertyName("text")]
	public string Text { get; set; }
	/// <summary>
	/// Constructor that maps a generic general message to an LCD general message.
	/// </summary>
	/// <param name="message"></param>
	public LcdGeneralMessage(GeneralMessage message)
	{
		Text = message.Text;
		BlocksRealTime = message.BlockRealtime;
	}
}

