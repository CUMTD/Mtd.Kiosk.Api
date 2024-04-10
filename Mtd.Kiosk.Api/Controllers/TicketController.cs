using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mtd.Kiosk.Infrastructure.EfCore;

namespace Mtd.Kiosk.Api.Controllers
{
	[Route("api/ticket")]
	[ApiController]
	public class TicketController : ControllerBase
	{
		private readonly KioskContext _context;
		private readonly ILogger<TicketController> _logger;

		public TicketController(KioskContext context, ILogger<TicketController> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpGet("{TicketId}")]
		public async Task<IActionResult> GetTicket(string TicketId)
		{
			var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == TicketId);
			if (ticket == null)
			{
				_logger.LogError("Ticket not found: {TicketId}", TicketId);
				return NotFound();
			}

			return Ok(ticket);
		}

		[HttpGet("all")]
		public async Task<IActionResult> GetAllTickets()
		{
			var tickets = await _context.Tickets.ToListAsync();
			return Ok(tickets);
		}

		[HttpPost]
		public async Task<IActionResult> CreateTicket(Core.Entities.Ticket ticket)
		{
			try
			{
				_context.Tickets.Add(ticket);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating ticket");
				return BadRequest();
			}
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetTicket), new { TicketId = ticket.Id }, ticket);
		}

		[HttpPut("{TicketId}")]
		public async Task<IActionResult> UpdateTicket(string TicketId, Core.Entities.Ticket ticket)
		{
			if (TicketId != ticket.Id)
			{
				return BadRequest();
			}

			// update ticket with new values
			var ticketToUpdate = await _context.Tickets.FindAsync(TicketId);
			if (ticketToUpdate == null)
			{
				return NotFound();
			}

			ticketToUpdate.Status = ticket.Status;
			ticketToUpdate.CloseDate = ticket.CloseDate;
			ticketToUpdate.OpenDate = ticket.OpenDate;
			ticketToUpdate.Description = ticket.Description;
			await _context.SaveChangesAsync();

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!TicketExists(TicketId))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}

		[HttpDelete("{TicketId}")]
		public async Task<IActionResult> DeleteTicket(string TicketId)
		{
			var ticket = await _context.Tickets.FindAsync(TicketId);
			if (ticket == null)
			{
				return NotFound();
			}

			_context.Tickets.Remove(ticket);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool TicketExists(string ticketId)
		{
			return _context.Tickets.Any(e => e.Id == ticketId);

		}
	}
}
