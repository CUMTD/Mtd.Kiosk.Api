using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.RealTime;

namespace Mtd.Kiosk.Api.Controllers
{
	public class MessagingController : ControllerBase
	{
		private readonly RealTimeClient _realTimeClient;
		private readonly ILogger<DeparturesController> _logger;
		public MessagingController(RealTimeClient realTimeClient, ILogger<DeparturesController> logger)
		{
			_realTimeClient = realTimeClient;
			_logger = logger;
		}

		[HttpGet("/general-messaging")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult> GetGeneralMessages(CancellationToken cancellationToken)
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
}
