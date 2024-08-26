using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.RealTime;
using Mtd.Kiosk.RealTime.Entities;

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
	/// <summary>
	/// Get general messages for an LCD display.
	/// </summary>
	/// <param name="stopId">The stop id to get general messages for.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>A GeneralMessage object, 204 if no messages.</returns>
	[HttpGet("/general-messaging/lcd")]
	[ProducesResponseType<GeneralMessage>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<GeneralMessage>> GetLcdGeneralMessages([FromQuery] string stopId, CancellationToken cancellationToken)
	{
		// fetch new general messages from the real-time API.
		var currentGeneralMessages = await _realTimeClient.GetGeneralMessagesAsync(cancellationToken);

		if (currentGeneralMessages != null && currentGeneralMessages.Length > 0)
		{
			var generalMessage = currentGeneralMessages
				.OrderByDescending(gm => gm.BlockRealtime)
				.FirstOrDefault(gm => gm.StopIds != null && gm.StopIds.Contains(stopId));

			if (generalMessage != null)
			{
				return Ok(generalMessage);
			}
		}

		return NoContent();
	}
}
