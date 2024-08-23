using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Api.Config;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Controllers;

[Route("kiosk")]
[ApiController]
public class KioskController : ControllerBase
{
	private readonly IKioskRepository _kioskRepository;
	private readonly IHeartbeatRepository _heartbeatRepository;
	private readonly ITicketRepository _ticketRepository;
	private readonly ILogger<KioskController> _logger;
	private readonly ApiConfiguration _apiConfiguration;

	public KioskController(IKioskRepository kioskRepository, IHeartbeatRepository heartbeatRepository, ITicketRepository ticketRepository, ILogger<KioskController> logger, IOptions<ApiConfiguration> apiConfiguration)
	{

		_kioskRepository = kioskRepository ?? throw new ArgumentNullException(nameof(kioskRepository));
		_heartbeatRepository = heartbeatRepository ?? throw new ArgumentNullException(nameof(heartbeatRepository));
		_ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_apiConfiguration = apiConfiguration.Value ?? throw new ArgumentNullException(nameof(apiConfiguration));
	}

	[HttpGet("{kioskId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
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

	[HttpGet("all")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<Ticket>>> GetAllKiosks(CancellationToken cancellationToken)
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

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
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

	[HttpGet("all/health")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> GetAllKioskHealth(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting all kiosk health");
		// get all kiosks, then get health for each kiosk, then return json object with health status of each kiosk
		var kiosks = await _kioskRepository.GetAllAsync(true, cancellationToken);

		// for each kiosk, get health status with KioskHealth (avoid DbContext threading issues)
		var kioskHealthTasks = kiosks.Select(k => KioskHealth(k.Id, cancellationToken).Result).ToArray();
		return Ok(kioskHealthTasks);

	}

	[HttpGet("{kioskId}/health")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> GetKioskHealth(string kioskId, CancellationToken cancellationToken)
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

	[NonAction]
	public async Task<object> KioskHealth(string kioskId, CancellationToken cancellationToken)
	{
		_logger.LogTrace("Getting health for kiosk: {kioskId}", kioskId);

		var buttonHealth = await CalculateHealth(kioskId, HeartbeatType.Button, cancellationToken);
		var ledHealth = await CalculateHealth(kioskId, HeartbeatType.LED, cancellationToken);
		var lcdHealth = await CalculateHealth(kioskId, HeartbeatType.LCD, cancellationToken);

		var openTicketCount = await _ticketRepository.GetOpenTicketCountAsync(kioskId, cancellationToken);

		// return json object with health status of each component
		return new
		{
			// return max health status of all components
			kioskId = kioskId,
			overallHealth = new[] { buttonHealth, ledHealth, lcdHealth }.Max(),
			healthStatuses = new
			{
				button = buttonHealth,
				led = ledHealth,
				lcd = lcdHealth
			},
			openTicketCount = openTicketCount
		};
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

	[HttpGet("{kioskId}/tickets")]
	[ProducesResponseType(StatusCodes.Status200OK)]
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
