using IpDisplaysSoapService;
using Microsoft.AspNetCore.Mvc;
using Mtd.Led.Soap;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// API methods related to previewing the content of LED signs.
/// </summary>
[Route("led-preview")]
[ApiController]
public class LedPreviewController : ControllerBase
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<LedPreviewController> _logger;
	/// <summary>
	/// Constructor for LedPreviewController.
	/// </summary>
	/// <param name="httpClient"></param>
	/// <param name="logger"></param>
	public LedPreviewController(HttpClient httpClient, ILogger<LedPreviewController> logger)
	{
		ArgumentNullException.ThrowIfNull(httpClient);
		ArgumentNullException.ThrowIfNull(logger);

		_httpClient = httpClient;
		_logger = logger;
	}

	/// <summary>
	/// Gets a PNG image of the currently displaying content for the LED at the provided address.
	/// </summary>
	/// <param name="ledIp">The IP address of the LED to preview.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A PNG image representing the current contents of the LED sign.</returns>
	[HttpGet("")]
	[ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	[Produces("image/png")]
	public async Task<ActionResult<FileContentResult>> GetLedPreviewImage([FromQuery] string ledIp, CancellationToken cancellationToken)
	{
		// TODO: We should't hard code this sort of thing in.
		if (ledIp != "10.128.17.35" && !ledIp.StartsWith("192.168."))
		{
			return BadRequest();
		}

		GetScreenSnapshotResponse response;
		try
		{
			var config = new IpDisplaysSoapConfig(ledIp, TimeSpan.FromSeconds(10));
			var soapClient = config.GetSoapClient();
			response = await soapClient.GetScreenSnapshotAsync(new GetScreenSnapshotRequest());
		}
		catch (TimeoutException)
		{
			_logger.LogWarning("Timeout getting Led preview image for {ip}", ledIp);
			return NotFound("Timeout getting LED preview.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting Led preview image at {ip}", ledIp);
			return StatusCode(500);
		}

		if (response == null)
		{
			_logger.LogWarning("Got a null response from SOAP client for LED preview of {ip}", ledIp);
			return StatusCode(500);
		}

		//assemble the link to the image
		var imageLink = $"http://{ledIp}/{response.fileName.Replace("\\", "/")}";

		try
		{
			//download the image and return it
			var image = await _httpClient.GetByteArrayAsync(imageLink, cancellationToken);
			return File(image, "image/png");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error downloading LED preview image at URL: {url}", imageLink);
			return StatusCode(500);
		}
	}
}
