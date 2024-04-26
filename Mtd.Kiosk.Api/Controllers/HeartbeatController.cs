using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Controllers
{
	[Route("heartbeat")]
	[ApiController]
	public class HeartbeatController(IHeartbeatRepository heartbeatRepository, ILogger<HeartbeatController> logger) : ControllerBase
	{
		private readonly IHeartbeatRepository _heartbeatRepository = heartbeatRepository ?? throw new ArgumentNullException(nameof(heartbeatRepository));
		private readonly ILogger<HeartbeatController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<Ticket>> PostHeartbeat([FromBody] NewHeartbeatModel newHeartbeatModel, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Recieved heartbeat for kiosk: {KioskId}", newHeartbeatModel.KioskId);
			var heartbeat = newHeartbeatModel.ToHeartbeat();
			try
			{
				await _heartbeatRepository.AddAsync(heartbeat, cancellationToken);
				await _heartbeatRepository.CommitChangesAsync(cancellationToken);
			}
			// not found
			catch (InvalidOperationException ex)
			{
				_logger.LogWarning(ex, "Kiosk not found: {KioskId}", newHeartbeatModel.KioskId);
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error saving heartbeat");
				return StatusCode(500);
			}

			_logger.LogInformation("Heartbeat added for kiosk: {KioskId}", newHeartbeatModel.KioskId);
			return Ok();
		}
	}
}
