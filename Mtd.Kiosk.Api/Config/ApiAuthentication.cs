using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Config
{

	public class ApiAuthentication
	{
		[Required]
		public required string[] ApiKeys { get; set; }
	}
}
