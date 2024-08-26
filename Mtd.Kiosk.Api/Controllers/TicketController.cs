using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// Controller for tickets.
/// </summary>
/// <param name="ticketRepository"></param>
/// <param name="logger"></param>
[Route("tickets")]
[ApiController]
public class TicketController(ITicketRepository ticketRepository, ILogger<TicketController> logger) : ControllerBase
{
	private readonly ITicketRepository _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
	private readonly ILogger<TicketController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	/// <summary>
	/// Gets a ticket by its id.
	/// </summary>
	/// <param name="ticketId">The id of the ticket to get</param>
	/// <param name="cancellationToken"></param>
	/// <returns>A ticket.</returns>
	[HttpGet("{ticketId}")]
	[ProducesResponseType<Ticket>(StatusCodes.Status200OK)]
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

	/// <summary>
	/// Gets all tickets.
	/// </summary>
	/// <param name="includeClosed">Whether to include closed tickets.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of Tickets.</returns>
	[HttpGet("")]
	[ProducesResponseType<IEnumerable<Ticket>>(StatusCodes.Status200OK)]
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
	/// <summary>
	/// Creates a new ticket in the database.
	/// </summary>
	/// <param name="newTicketModel">The new ticket to be created.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>The ticket, if successful.</returns>
	[HttpPost]
	[ProducesResponseType<Ticket>(StatusCodes.Status201Created)]
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

	/// <summary>
	/// Updates a ticket's status to a new TicketStatusType.
	/// </summary>
	/// <param name="ticketId">The ticket id to be updated</param>
	/// <param name="newStatus">The new TicketStatusType of the ticket</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpPatch("{ticketId}/status")]
	[ProducesResponseType<Ticket>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Ticket>> UpdateTicket(string ticketId, [FromQuery] TicketStatusType newStatus, CancellationToken cancellationToken)
	{

		Ticket ticket;
		try
		{
			ticket = await _ticketRepository.GetByIdentityAsync(ticketId, cancellationToken);
			ticket.Status = newStatus;
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

	/// <summary>
	/// Adds a comment to a ticket.
	/// </summary>
	/// <param name="ticketId">The id of the ticket to add a comment to</param>
	/// <param name="newTicketNoteModel">The new ticket note to add</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpPost("{ticketId}/comment")]
	[ProducesResponseType<Ticket>(StatusCodes.Status200OK)]
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
