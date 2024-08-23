using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Controllers;

[Route("ticket")]
[ApiController]
public class TicketController(ITicketRepository ticketRepository, ILogger<TicketController> logger) : ControllerBase
{
	private readonly ITicketRepository _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
	private readonly ILogger<TicketController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	[HttpGet("{ticketId}")]
	[ProducesResponseType(typeof(Ticket), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Ticket>> GetTicket(string ticketId, CancellationToken cancellationToken)
	{
		Ticket ticket;
		try
		{
			ticket = await _ticketRepository.GetByIdentityAsync(ticketId, cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Ticket not found: {ticketId}", ticketId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting ticket: {ticketId}", ticketId);
			return StatusCode(500);
		}

		return Ok(ticket);
	}

	[HttpGet("all")]
	[ProducesResponseType(typeof(IEnumerable<Ticket>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<Ticket>>> GetAllTickets([FromQuery] bool includeClosed, CancellationToken cancellationToken)
	{

		IReadOnlyCollection<Ticket> tickets;

		try
		{
			if (includeClosed)
			{
				tickets = await _ticketRepository.GetAllAsync(cancellationToken);
			}
			else
			{
				tickets = await _ticketRepository.GetAllOpenTicketsAsync(cancellationToken);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting tickets");
			return StatusCode(500);
		}

		return Ok(tickets);

	}
	[HttpPost]
	[ProducesResponseType(typeof(Ticket), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Ticket>> CreateTicket(NewTicketModel newTicketModel, CancellationToken cancellationToken)
	{
		var ticket = newTicketModel.ToTicket();
		try
		{
			await _ticketRepository.AddAsync(ticket, cancellationToken);
			await _ticketRepository.CommitChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating ticket");
			return StatusCode(500);
		}

		return CreatedAtAction(nameof(GetTicket), new { TicketId = ticket.Id }, ticket);
	}

	[HttpPatch("{ticketId}/status/{status}")]
	[ProducesResponseType(typeof(Ticket), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Ticket>> UpdateTicket(string ticketId, TicketStatusType status, CancellationToken cancellationToken)
	{

		Ticket ticket;
		try
		{
			ticket = await _ticketRepository.GetByIdentityAsync(ticketId, cancellationToken);
			ticket.Status = status;
			await _ticketRepository.CommitChangesAsync(cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Ticket not found: {ticketId}", ticketId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating ticket: {ticketId}", ticketId);
			return StatusCode(500);
		}

		return Ok(ticket);
	}

	[HttpPost("{ticketId}/comment")]
	[ProducesResponseType(typeof(Ticket), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Ticket>> AddComment([FromRoute] string ticketId, [FromBody] NewTicketNoteModel newTicketNoteModel, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Adding comment to ticket: {ticketId}", ticketId);
		Ticket ticket;
		var note = newTicketNoteModel.ToTicketNote(ticketId);
		try
		{
			ticket = await _ticketRepository.GetByIdentityAsync(ticketId, cancellationToken);
			ticket.Notes.Add(note);
			await _ticketRepository.CommitChangesAsync(cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Ticket not found: {ticketId}", ticketId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding comment to ticket: {ticketId}", ticketId);
			return StatusCode(500);
		}

		return Ok(ticket);
	}
}
