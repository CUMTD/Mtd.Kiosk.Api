namespace Mtd.Kiosk.Api.Models.Enums;

/// <summary>
/// A type of departure display.
/// </summary>
public enum DepartureTypeEnum
{
	/// <summary>
	/// An ipDisplays LED sign atop a kiosk.
	/// </summary>
	Led,
	/// <summary>
	/// A "PUSH FOR AUDIO" annunciator button.
	/// </summary>
	Button,
	/// <summary>
	/// The LCD screen on a kiosk.
	/// </summary>
	Lcd
}
