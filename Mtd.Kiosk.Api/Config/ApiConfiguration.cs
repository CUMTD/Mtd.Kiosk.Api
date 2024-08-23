using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Config;

public class ApiConfiguration
{
	[Required]
	[Range(1, int.MaxValue)]
	public required int WarningHeartbeatThresholdMinutes { get; set; }
	[Required]
	[Range(1, int.MaxValue)]
	public required int CriticalHeartbeatThresholdMinutes { get; set; }
}
