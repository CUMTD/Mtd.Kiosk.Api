using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// Controller for ticket notes.
/// </summary>
/// <param name="ticketNoteRepository"></param>
/// <param name="logger"></param>
[Route("ticket-notes")]
[ApiController]
public class TicketNoteController(ITicketNoteRepository ticketNoteRepository, ILogger<TicketNoteController> logger) : ControllerBase
{
	private readonly ITicketNoteRepository _ticketNoteRepository = ticketNoteRepository ?? throw new ArgumentNullException(nameof(ticketNoteRepository));
	private readonly ILogger<TicketNoteController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	/// <summary>
	/// Delete a ticket note/comment.
	/// </summary>
	/// <param name="noteId">The note id to delete</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpDelete("{noteId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult> DeleteTicketNoteAsync(string noteId, CancellationToken cancellationToken)
	{
		TicketNote ticketNote;

		try
		{

			ticketNote = await _ticketNoteRepository.GetByIdentityAsync(noteId, cancellationToken);
			ticketNote.Deleted = true;
			await _ticketNoteRepository.CommitChangesAsync(cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Ticket note not found: {noteId}", noteId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting ticket note");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		await _ticketNoteRepository.DeleteAsync(ticketNote, cancellationToken);
		return NoContent();
	}

	/// <summary>
	/// Update a ticket note/comment with the provided markdown body.
	/// </summary>
	/// <param name="noteId">The note/comment to update.</param>
	/// <param name="updatedTicketModel">An UpdateTicketModel with new data</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpPatch("{noteId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> UpdateTicketNoteAsync(string noteId, [FromBody] UpdateTicketModel updatedTicketModel, CancellationToken cancellationToken)
	{
		var ticketNote = await _ticketNoteRepository.GetByIdentityAsync(noteId, cancellationToken);

		if (ticketNote == null)
		{
			return NotFound();
		}

		try
		{
			ticketNote.MarkdownBody = updatedTicketModel.MarkdownBody;

			await _ticketNoteRepository.CommitChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating ticket note");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		return Ok();
	}
}
