using Mtd.Kiosk.RealTime.Entities;

namespace Mtd.Kiosk.Api.Models
{
	public class LedDepartureResponseModel
	{
		public IReadOnlyCollection<LedDeparture> Departures { get; set; }

		public LedDepartureResponseModel(IReadOnlyCollection<LedDeparture> departures)
		{
			Departures = departures;
		}
	}

	public class LedDeparture
	{
		public string Route { get; set; }
		public string Time { get; set; }

		public LedDeparture(string route, string time)
		{
			Route = route;
			Time = time;
		}

		public LedDeparture(Departure departure)
		{

			Route = departure.FriendlyRouteName.ToUpper();
			Time = departure.DepartsIn;
		}
	}
}

