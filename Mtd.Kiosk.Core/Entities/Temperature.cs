
using Mtd.Core.Entities;

namespace Mtd.Kiosk.Core.Entities;

public class Temperature : IEntity
{
	public string KioskId { get; set; } = string.Empty;

	public DateTimeOffset Timestamp { get; set; }

	public byte TempFahrenheit { get; set; }

	public byte RelHumidity { get; set; }

	public Temperature(string kioskId, byte tempFahrenheit, byte relHumidity) : this()
	{
		KioskId = kioskId;
		TempFahrenheit = tempFahrenheit;
		RelHumidity = relHumidity;
		Timestamp = DateTimeOffset.Now.ToLocalTime();
	}

	public Temperature()
	{
		Timestamp = DateTimeOffset.Now.ToLocalTime();
	}
}
