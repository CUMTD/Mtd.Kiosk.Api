using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for the response of the LCD departure endpoint.
/// </summary>
/// <remarks>
/// Constructor for the LCD departure response model.
/// </remarks>
/// <param name="groupedDepartures">Up to three upcoming departures for the current route.</param>
/// <param name="generalMessage"></param>
public class LcdDepartureResponseModel(IEnumerable<LcdDepartureGroup> groupedDepartures, LcdGeneralMessage? generalMessage)
{
	/// <summary>
	/// A list of upcoming departures (up to 3).
	/// </summary>
	[JsonPropertyName("groupedDepartures")]
	public IReadOnlyCollection<LcdDepartureGroup> Routes { get; set; } = groupedDepartures.ToImmutableArray();

	/// <summary>
	/// Current general message to display, if any.
	/// </summary>
	[JsonPropertyName("generalMessage")]
	public LcdGeneralMessage? GeneralMessage { get; set; } = generalMessage;
}

