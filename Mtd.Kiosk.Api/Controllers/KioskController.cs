using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Infrastructure.EfCore;

namespace Mtd.Kiosk.Api.Controllers
{
	[Route("api/kiosk")]
	[ApiController]
	public class KioskController : ControllerBase
	{
		private readonly KioskContext _context;
		private readonly ILogger<KioskController> _logger;

		public KioskController(KioskContext context, ILogger<KioskController> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpGet]
		public IActionResult Get()
		{
			return Ok("Kiosk API is up");
		}

		[HttpGet("{KioskId}")]
		public async Task<IActionResult> GetKiosk(string KioskId)
		{
			var kiosk = await _context.Kiosks.FirstOrDefaultAsync(k => k.Id == KioskId);
			if (kiosk == null)
			{
				_logger.LogError("Kiosk not found: {KioskId}", KioskId);
				return NotFound();
			}

			return Ok(kiosk);
		}
		[HttpGet("all")]
		public async Task<IActionResult> GetAllKiosks()
		{
			var kiosks = await _context.Kiosks.ToListAsync();
			return Ok(kiosks);
		}

		[HttpPost]
		public async Task<IActionResult> CreateKiosk(Core.Entities.Kiosk kiosk)
		{
			_context.Kiosks.Add(kiosk);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetKiosk), new { KioskId = kiosk.Id }, kiosk);
		}

		[HttpGet("{KioskId}/health")]
		public async Task<IActionResult> GetKioskHealth(string KioskId)
		{
			var kiosk = await _context.Kiosks.FirstOrDefaultAsync(k => k.Id == KioskId);

			// random health 
			var random = new Random();

			// random health statuses for demo purposes
			var buttonHealth = random.Next(0, 3);
			var ledHealth = random.Next(0, 3);
			var lcdHealth = random.Next(0, 3);

			// return json object with health status of each component
			return Ok(new
			{
				// return max health status of all components
				overallHealth = Math.Max(buttonHealth, Math.Max(ledHealth, lcdHealth)),
				healthStatuses = new
				{
					button = buttonHealth,
					led = ledHealth,
					lcd = lcdHealth
				}

			});
		}

		[HttpGet("{KioskId}/health/button")]
		public async Task<IActionResult> GetKioskButtonHealth(string KioskId)
		{
			var kiosk = await _context.Kiosks.FirstOrDefaultAsync(k => k.Id == KioskId);

			// TODO: implement button health check
			return Ok(HealthStatus.Healthy);
		}

		// repeat for LED and LCD
		[HttpGet("{KioskId}/health/led")]
		public async Task<IActionResult> GetKioskLedHealth(string KioskId)
		{
			var kiosk = await _context.Kiosks.FirstOrDefaultAsync(k => k.Id == KioskId);

			// TODO: implement led health check
			return Ok(HealthStatus.Healthy);
		}

		[HttpGet("{KioskId}/health/lcd")]
		public async Task<IActionResult> GetKioskLcdHealth(string KioskId)
		{
			var kiosk = await _context.Kiosks.FirstOrDefaultAsync(k => k.Id == KioskId);

			// TODO: implement lcd health check
			return Ok(HealthStatus.Healthy);
		}
	}
}
