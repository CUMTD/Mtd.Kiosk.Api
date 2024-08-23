using Mtd.Kiosk.RealTime.Entities;
using Mtd.Stopwatch.Core.Entities.Schedule;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Enums
{
	public class LcdDepartureResponseModel
	{
		[JsonPropertyName("routes")]
		public IReadOnlyCollection<LcdDeparture> Routes { get; set; }

		[JsonPropertyName("generalMessage")]
		public LcdGeneralMessage? GeneralMessage { get; set; }

		public LcdDepartureResponseModel(IEnumerable<LcdDeparture> routes, LcdGeneralMessage? generalMessage)
		{
			Routes = routes.ToImmutableArray();
			GeneralMessage = generalMessage;
		}
	}

	public class LcdDeparture
	{
		[JsonPropertyName("number")]
		public string? Number { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("direction")]
		public string Direction { get; set; }

		[JsonPropertyName("foregroundHexColor")]
		public string ForegroundHexColor { get; set; }

		[JsonPropertyName("backgroundHexColor")]
		public string BackgroundHexColor { get; set; }

		[JsonPropertyName("sortOrder")]
		public int SortOrder { get; set; }

		[JsonPropertyName("departureTimes")]
		public List<LcdDepartureTime> DepartureTimes { get; set; }

		public LcdDeparture(PublicRoute publicRoute, List<LcdDepartureTime> lcdDepartures, string direction)
		{
			Number = publicRoute.RouteNumber;
			Name = publicRoute.PublicRouteGroup.RouteName;
			Direction = direction;
			ForegroundHexColor = $"#{publicRoute.PublicRouteGroup.HexTextColor}";
			BackgroundHexColor = $"#{publicRoute.PublicRouteGroup.HexColor}";
			;
			DepartureTimes = lcdDepartures;
			SortOrder = publicRoute.PublicRouteGroup.SortNumber;
		}
	}

	public class LcdDepartureTime
	{
		[JsonPropertyName("time")]
		public string Time { get; set; }

		[JsonPropertyName("isRealTime")]
		public bool IsRealTime { get; set; }

		[JsonPropertyName("isHopper")]
		public bool IsHopper { get; set; }

		public LcdDepartureTime(Departure departure)
		{
			Time = departure.DepartsIn;
			IsRealTime = departure.IsRealTime;
			IsHopper = departure.IsHopper;
		}
	}

	public class LcdGeneralMessage
	{
		[JsonPropertyName("blocksRealtime")]
		public bool BlocksRealTime { get; set; }
		[JsonPropertyName("text")]
		public string Text { get; set; }
		public LcdGeneralMessage(GeneralMessage message)
		{
			Text = message.Text;
			BlocksRealTime = message.BlockRealtime;

		}
	}
}

