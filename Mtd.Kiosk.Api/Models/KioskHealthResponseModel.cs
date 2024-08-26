using Mtd.Kiosk.Core.Entities;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models
{
	/// <summary>
	/// The health of a kiosk.
	/// </summary>
	public class KioskHealthResponseModel
	{
		/// <summary>
		/// The id of the kiosk.
		/// </summary>
		public string KioskId { get; set; }

		/// <summary>
		/// The overall health of the kiosk.
		/// </summary>
		public HealthStatus OverallHealth { get; set; }

		/// <summary>
		/// A collection of health statuses for each component.
		/// </summary>
		[JsonPropertyName("healthStatuses")]
		public IndividualHealthStatuses IndividualHealthStatuses { get; set; }

		/// <summary>
		/// The number of open tickets for the kiosk.
		/// </summary>
		[JsonPropertyName("openTicketCount")]
		public int OpenTicketCount { get; set; }

		/// <summary>
		/// Constructor for KioskHealthResponseModel.
		/// </summary>
		/// <param name="kioskId"></param>
		/// <param name="overallHealth"></param>
		/// <param name="healthStatuses"></param>
		/// <param name="openTicketCount"></param>
		public KioskHealthResponseModel(string kioskId, HealthStatus overallHealth, IndividualHealthStatuses healthStatuses, int openTicketCount)
		{
			KioskId = kioskId;
			OverallHealth = overallHealth;
			IndividualHealthStatuses = healthStatuses;
			OpenTicketCount = openTicketCount;
		}
	}

	/// <summary>
	/// A collection of health statuses for each component.
	/// </summary>
	public class IndividualHealthStatuses
	{
		/// <summary>
		/// The health status of the button.
		/// </summary>
		[JsonPropertyName("button")]
		public HealthStatus ButtonStatus { get; set; }

		/// <summary>
		/// The health status of the LED.
		/// </summary>
		[JsonPropertyName("led")]
		public HealthStatus LedStatus { get; set; }

		/// <summary>
		/// The health status of the LCD.
		/// </summary>
		[JsonPropertyName("lcd")]
		public HealthStatus LcdStatus { get; set; }

		/// <summary>
		/// Constructor for IndividualHealthStatuses.
		/// </summary>
		/// <param name="buttonStatus"></param>
		/// <param name="ledStatus"></param>
		/// <param name="lcdStatus"></param>
		public IndividualHealthStatuses(HealthStatus buttonStatus, HealthStatus ledStatus, HealthStatus lcdStatus)
		{
			ButtonStatus = buttonStatus;
			LedStatus = ledStatus;
			LcdStatus = lcdStatus;
		}
	}
}

