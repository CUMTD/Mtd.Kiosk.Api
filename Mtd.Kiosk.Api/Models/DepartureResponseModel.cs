using Mtd.Kiosk.RealTime.Entities;

namespace Mtd.Kiosk.Api.Models
{
	public class DepartureResponseModel
	{
		public IReadOnlyCollection<SimpleDeparture> Departures { get; set; }

		public DepartureResponseModel(IReadOnlyCollection<SimpleDeparture> departures)
		{
			Departures = departures;
		}

	}

	public class SimpleDeparture
	{
		public string Route { get; set; }
		public string Time { get; set; }

		public SimpleDeparture(string route, string time)
		{
			Route = route;
			Time = time;
		}

		public SimpleDeparture(Departure departure)
		{
			Route = departure.FriendlyRouteName;
			Time = departure.DepartsIn;
		}

	}
}

