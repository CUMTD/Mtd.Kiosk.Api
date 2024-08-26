using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Api.Config;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// A collection of endpoints for interacting and modifying kiosk data.
/// </summary>
[Route("kiosks")]
[ApiController]
public class KioskController : ControllerBase
{
	private readonly IKioskRepository _kioskRepository;
	private readonly IHeartbeatRepository _heartbeatRepository;
	private readonly ITicketRepository _ticketRepository;
	private readonly ILogger<KioskController> _logger;
	private readonly ApiConfiguration _apiConfiguration;

	/// <summary>
	/// Constructor for the Kiosk controller.
	/// </summary>
	/// <param name="kioskRepository"></param>
	/// <param name="heartbeatRepository"></param>
	/// <param name="ticketRepository"></param>
	/// <param name="logger"></param>
	/// <param name="apiConfiguration"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public KioskController(IKioskRepository kioskRepository, IHeartbeatRepository heartbeatRepository, ITicketRepository ticketRepository, ILogger<KioskController> logger, IOptions<ApiConfiguration> apiConfiguration)
	{

		_kioskRepository = kioskRepository ?? throw new ArgumentNullException(nameof(kioskRepository));
		_heartbeatRepository = heartbeatRepository ?? throw new ArgumentNullException(nameof(heartbeatRepository));
		_ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_apiConfiguration = apiConfiguration.Value ?? throw new ArgumentNullException(nameof(apiConfiguration));
	}

	/// <summary>
	/// Get a kiosk by its id.
	/// </summary>
	/// <param name="kioskId">The kiosk id to get.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>A Kiosk object</returns>
	[HttpGet("{kioskId}")]
	[ProducesResponseType<Core.Entities.Kiosk>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Core.Entities.Kiosk>> GetKiosk(string kioskId, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting kiosk: {kioskId}", kioskId);
		Core.Entities.Kiosk kiosk;
		try
		{
			kiosk = await _kioskRepository.GetByIdentityWithTicketsAsync(kioskId, cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Kiosk not found: {kioskId}", kioskId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting kiosk: {kioskId}", kioskId);
			return StatusCode(500);
		}

		return Ok(kiosk);
	}

	/// <summary>
	/// Get all kiosks.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of all kiosks.</returns>
	[HttpGet("")]
	[ProducesResponseType<IEnumerable<Core.Entities.Kiosk>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<Core.Entities.Kiosk>>> GetAllKiosks(CancellationToken cancellationToken)
	{
		IReadOnlyCollection<Core.Entities.Kiosk> kiosks;
		try
		{
			_logger.LogInformation("Getting all kiosks");
			kiosks = await _kioskRepository.GetAllAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting all kiosks");
			return StatusCode(500);
		}

		return Ok(kiosks);
	}

	/// <summary>
	/// Create a new kiosk.
	/// </summary>
	/// <param name="kiosk">The kiosk Object to add.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>The kiosk, if successfully added.</returns>
	[HttpPost]
	[ProducesResponseType<Core.Entities.Kiosk>(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Core.Entities.Kiosk>> CreateKiosk(Core.Entities.Kiosk kiosk, CancellationToken cancellationToken)
	{
		try
		{
			await _kioskRepository.AddAsync(kiosk, cancellationToken);
			await _kioskRepository.CommitChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating kiosk");
			return StatusCode(500);
		}

		return CreatedAtAction(nameof(GetKiosk), new { KioskId = kiosk.Id }, kiosk);
	}

	// TODO: make an object for this

	/// <summary>
	/// Get all kiosk health statuses.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of KioskHealthResponseModel objects</returns>
	[HttpGet("health")]
	[ProducesResponseType<IEnumerable<KioskHealthResponseModel>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<KioskHealthResponseModel>>> GetAllKioskHealth(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting all kiosk health");
		// get all kiosks, then get health for each kiosk, then return json object with health status of each kiosk
		var kiosks = await _kioskRepository.GetAllAsync(true, cancellationToken);

		// for each kiosk, get health status with KioskHealth (avoid DbContext threading issues)
		var kioskHealthTasks = kiosks.Select(k => KioskHealth(k.Id, cancellationToken).Result).ToArray();
		return Ok(kioskHealthTasks);

	}

	/// <summary>
	/// Get the health status of a single kiosk.
	/// </summary>
	/// <param name="kioskId">The id of the kiosk to ge the status for.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpGet("{kioskId}/health")]
	[ProducesResponseType<KioskHealthResponseModel>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<KioskHealthResponseModel>> GetKioskHealth(string kioskId, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await KioskHealth(kioskId, cancellationToken));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting health for kiosk: {kioskId}", kioskId);
			return StatusCode(500);
		}
	}

	/// <summary>
	/// Get the health status of a kiosk.
	/// </summary>
	/// <param name="kioskId">The kiosk id to fetch the health for.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>A KioskHealthResponseModel object.</returns>
	[NonAction]
	public async Task<KioskHealthResponseModel> KioskHealth(string kioskId, CancellationToken cancellationToken)
	{
		_logger.LogTrace("Getting health for kiosk: {kioskId}", kioskId);

		var buttonHealth = await CalculateHealth(kioskId, HeartbeatType.Button, cancellationToken);
		var ledHealth = await CalculateHealth(kioskId, HeartbeatType.LED, cancellationToken);
		var lcdHealth = await CalculateHealth(kioskId, HeartbeatType.LCD, cancellationToken);

		var openTicketCount = await _ticketRepository.GetOpenTicketCountAsync(kioskId, cancellationToken);

		// return json object with health status of each component
		var overallHealth = new[] { buttonHealth, ledHealth, lcdHealth }.Max();
		var healthStatuses = new IndividualHealthStatuses(buttonHealth, ledHealth, lcdHealth);

		return new KioskHealthResponseModel(kioskId, overallHealth, healthStatuses, openTicketCount);
	}

	// calculate health for each possible HeartbeatType
	private async Task<HealthStatus> CalculateHealth(string kioskId, HeartbeatType heartbeatType, CancellationToken cancellationToken)
	{
		IEnumerable<Heartbeat> heartbeats;
		Heartbeat lastHeartbeat;

		try
		{
			heartbeats = await _heartbeatRepository.GetByIdentityAndHeartbeatTypeAsync(kioskId, heartbeatType, cancellationToken);
			if (heartbeats != null)
			{
				lastHeartbeat = heartbeats.OrderByDescending(h => h.Timestamp).First();
			}
			else
			{
				return HealthStatus.Unknown;
			}
		}
		catch (Exception ex)
		{
			// TODO: this was noisy in development
			_logger.LogError(ex, "Error getting heartbeats for kiosk: {kioskId}", kioskId);
			return HealthStatus.Unknown;
		}

		if (lastHeartbeat == null)
		{
			return HealthStatus.Unknown;
		}

		var timeSinceLastHeartbeat = DateTime.UtcNow - lastHeartbeat.Timestamp;
		if (timeSinceLastHeartbeat.TotalMinutes > _apiConfiguration.WarningHeartbeatThresholdMinutes)
		{
			if (timeSinceLastHeartbeat > TimeSpan.FromMinutes(_apiConfiguration.CriticalHeartbeatThresholdMinutes))
			{
				return HealthStatus.Critical;
			}

			return HealthStatus.Warning;
		}

		return HealthStatus.Healthy;
	}

	/// <summary>
	/// Get tickets for a kiosk id.
	/// </summary>
	/// <param name="KioskId">The kiosk id to fetc htickets for.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of Ticket objects.</returns>
	[HttpGet("{kioskId}/tickets")]
	[ProducesResponseType<IEnumerable<Ticket>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsByKiosk(string KioskId, CancellationToken cancellationToken)
	{
		IReadOnlyCollection<Ticket> tickets;
		try
		{
			tickets = await _ticketRepository.GetByKioskIdAsync(KioskId, cancellationToken);
			// split into open and closed tickets
			if (tickets != null)
			{
				tickets = tickets.OrderByDescending(t => t.Status == TicketStatusType.Open).ThenByDescending(t => t.OpenDate).ToArray();

			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting tickets for kiosk: {kioskId}", KioskId);
			return StatusCode(500);
		}

		return Ok(tickets);
	}
}
