using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.RealTime;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// Controller for messaging.
/// </summary>
public class MessagingController : ControllerBase
{
	private readonly RealTimeClient _realTimeClient;
	private readonly ILogger<DeparturesController> _logger;
	/// <summary>
	/// Constructor for MessagingController.
	/// </summary>
	/// <param name="realTimeClient"></param>
	/// <param name="logger"></param>
	public MessagingController(RealTimeClient realTimeClient, ILogger<DeparturesController> logger)
	{
		_realTimeClient = realTimeClient;
		_logger = logger;
	}

	/// <summary>
	/// Get general messages from the SIRI realtime endpoint.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>Currently active general messages</returns>
	[HttpGet("/general-messaging")]
	[ProducesResponseType<IReadOnlyCollection<SimpleGeneralMessage>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IReadOnlyCollection<SimpleGeneralMessage>>> GetGeneralMessages(CancellationToken cancellationToken)
	{

		var result = await _realTimeClient.GetGeneralMessagesAsync(cancellationToken);

		if (result == null)
		{
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		var response = new MessagingResponseModel(result);
		return Ok(response.GeneralMessages);
	}
}
