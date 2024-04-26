using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Api.Config;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Controllers
{
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

		[HttpGet("{KioskId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<Core.Entities.Kiosk>> GetKiosk(string KioskId, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Getting kiosk: {KioskId}", KioskId);
			Core.Entities.Kiosk kiosk;
			try
			{
				kiosk = await _kioskRepository.GetByIdentityWithTicketsAsync(KioskId, cancellationToken);
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogWarning(ex, "Kiosk not found: {KioskId}", KioskId);
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting kiosk: {KioskId}", KioskId);
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

		[HttpGet("{KioskId}/health")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult> GetKioskHealth(string KioskId, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Getting health for kiosk: {KioskId}", KioskId);

			var buttonHealth = await CalculateHealth(KioskId, HeartbeatType.Button, cancellationToken);
			var ledHealth = await CalculateHealth(KioskId, HeartbeatType.LED, cancellationToken);
			var lcdHealth = await CalculateHealth(KioskId, HeartbeatType.LCD, cancellationToken);

			var openTicketCount = await _ticketRepository.GetOpenTicketCountAsync(KioskId, cancellationToken);

			// return json object with health status of each component
			return Ok(new
			{
				// return max health status of all components
				overallHealth = new[] { buttonHealth, ledHealth, lcdHealth }.Max(),
				healthStatuses = new
				{
					button = buttonHealth,
					led = ledHealth,
					lcd = lcdHealth
				},
				openTicketCount = openTicketCount
			});
		}

		private async Task<HealthStatus> CalculateHealth(string KioskId, HeartbeatType HeartbeatType, CancellationToken cancellationToken)
		{
			IEnumerable<Heartbeat> heartbeats;
			Heartbeat lastHeartbeat;

			try
			{
				heartbeats = await _heartbeatRepository.GetByIdentityAndHeartbeatTypeAsync(KioskId, HeartbeatType, cancellationToken);
				if (heartbeats != null)
				{
					lastHeartbeat = heartbeats.OrderByDescending(h => h.Timestamp).FirstOrDefault();
				}
				else
				{
					return HealthStatus.Unknown;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting heartbeats for kiosk: {KioskId}", KioskId);
				return HealthStatus.Unknown;
			}

			if (lastHeartbeat == null)
			{
				return HealthStatus.Unknown;
			}

			var timeSinceLastHeartbeat = DateTime.UtcNow - lastHeartbeat.Timestamp;
			if (timeSinceLastHeartbeat > TimeSpan.FromMinutes(_apiConfiguration.WarningHeartbeatThresholdMinutes))
			{
				if (timeSinceLastHeartbeat > TimeSpan.FromMinutes(_apiConfiguration.CriticalHeartbeatThresholdMinutes))
				{
					return HealthStatus.Critical;
				}

				return HealthStatus.Warning;
			}

			return HealthStatus.Healthy;
		}

		[HttpGet("{KioskId}/tickets")]
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
					var openTickets =
						tickets.Where(t => t.Status == TicketStatusType.Open).ToList();
					var closedTickets = tickets.Where(t => t.Status == TicketStatusType.Resolved).ToList();





					openTickets.Sort((t1, t2) => t2.OpenDate.CompareTo(t1.OpenDate));
					closedTickets.Sort((t1, t2) => t2.OpenDate.CompareTo(t1.OpenDate));

					tickets = openTickets.Concat(closedTickets).ToList();





				}

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting tickets for kiosk: {KioskId}", KioskId);
				return StatusCode(500);
			}

			return Ok(tickets);
		}


	}
}
