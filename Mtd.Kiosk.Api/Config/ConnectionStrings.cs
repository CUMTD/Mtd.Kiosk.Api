namespace Mtd.Kiosk.Api.Config
{
	public class ConnectionStrings
	{

		// TODO: @Ryan, can I just get away with one connection string?
		public required string KioskConnection { get; set; }
		public required string StopwatchConnection { get; set; }
	}
}
