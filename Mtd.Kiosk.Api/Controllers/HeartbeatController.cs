using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// A collection of endpoints for interacting with kiosk heartbeat data.
/// </summary>
/// <param name="heartbeatRepository"></param>
/// <param name="logger"></param>
[ApiController]
[Produces("application/json")]
[Route("heartbeat")]
public class HeartbeatController(IHeartbeatRepository heartbeatRepository, ILogger<HeartbeatController> logger) : ControllerBase
{
	private readonly IHeartbeatRepository _heartbeatRepository = heartbeatRepository ?? throw new ArgumentNullException(nameof(heartbeatRepository));
	private readonly ILogger<HeartbeatController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	/// <summary>
	/// Adds a new kiosk heartbeat to the database.
	/// </summary>
	/// <param name="newHeartbeatModel">The heartbeat to add to the database.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpPost]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> PostHeartbeat([FromBody, Required] NewHeartbeatModel newHeartbeatModel, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Recieved heartbeat for kiosk: {KioskId}", newHeartbeatModel.KioskId);

		var heartbeat = newHeartbeatModel.ToHeartbeat();
		try
		{
			await _heartbeatRepository.AddAsync(heartbeat, cancellationToken);
			await _heartbeatRepository.CommitChangesAsync(cancellationToken);
		}
		catch (InvalidOperationException ex) // not found
		{
			_logger.LogWarning(ex, "Kiosk not found: {KioskId}", newHeartbeatModel.KioskId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error saving heartbeat for {kioskId}", newHeartbeatModel.KioskId);
			return StatusCode(500);
		}

		_logger.LogInformation("Heartbeat logged for kiosk: {KioskId}", newHeartbeatModel.KioskId);

		return Ok();
	}
}
