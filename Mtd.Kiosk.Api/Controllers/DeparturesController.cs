using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Enums;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.RealTime;

namespace Mtd.Kiosk.Api.Controllers
{
	[Route("api/departures")]
	[ApiController]
	public class DeparturesController : ControllerBase
	{
		private readonly RealTimeClient _realTimeClient;
		private readonly ILogger<DeparturesController> _logger;

		public DeparturesController(RealTimeClient realTimeClient, ILogger<DeparturesController> logger)
		{
			_realTimeClient = realTimeClient;
			_logger = logger;
		}

		[HttpGet("{stopId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult> GetDepartures(string stopId, [FromQuery] DepartureTypeEnum type, CancellationToken cancellationToken)
		{

			var result = await _realTimeClient.GetRealTimeForStop(stopId, cancellationToken);
			// sort result
			if (result != null)
			{
				result = result.OrderBy(x => x.MinutesTillDeparture).ToArray();
			}

			if (result == null)
			{
				return StatusCode(StatusCodes.Status500InternalServerError);
			}

			if (type == DepartureTypeEnum.led)
			{
				var departures = new List<SimpleDeparture>();

				// add max 4 departures
				for (int i = 0; i < 4 && i < result.Length; i++)
				{
					var departure = new SimpleDeparture(result[i]);
					departures.Add(departure);
				}

				return Ok(departures);

			}

			return Ok(result);
		}
	}
}
