using IpDisplaysSoapService;
using Microsoft.AspNetCore.Mvc;
using Mtd.Led.Soap;

namespace Mtd.Kiosk.Api.Controllers
{
	[Route("ledPreview")]
	[ApiController]
	public class LedPreviewController : ControllerBase
	{
		private readonly ILogger<LedPreviewController> _logger;

		public LedPreviewController(ILogger<LedPreviewController> logger)
		{
			_logger = logger;
		}

		[HttpGet("")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[Produces("image/png")]
		public async Task<ActionResult> GetLedPreviewImage([FromQuery] string ledIp, CancellationToken cancellationToken)
		{
			if (ledIp != "10.128.17.35" && !ledIp.StartsWith("192.168."))
			{
				return BadRequest();
			}

			var response = new GetScreenSnapshotResponse();
			try
			{

				var config = new IpDisplaysSoapConfig(ledIp, TimeSpan.FromSeconds(10));
				var soapClient = config.GetSoapClient();
				response = await soapClient.GetScreenSnapshotAsync(new GetScreenSnapshotRequest());
			}
			catch (TimeoutException)
			{
				_logger.LogWarning("Timeout getting Led preview image");
				return StatusCode(404);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting Led preview image");
				return StatusCode(500);
			}

			if (response == null)
			{
				return StatusCode(500);
			}

			//assemble the link to the image
			var imageLink = $"http://{ledIp}/{response.fileName.Replace("\\", "/")}";

			try
			{
				//download the image and return it
				using var httpClient = new HttpClient();
				var image = await httpClient.GetByteArrayAsync(imageLink);
				return File(image, "image/png");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error downloading Led preview image");
				return StatusCode(500);
			}
		}
	}
}
