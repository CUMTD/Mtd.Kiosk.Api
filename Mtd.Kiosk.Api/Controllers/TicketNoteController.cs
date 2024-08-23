using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Controllers
{
	[Route("ticket-note")]
	[ApiController]
	public class TicketNoteController(ITicketNoteRepository ticketNoteRepository, ILogger<TicketNoteController> logger) : ControllerBase
	{
		private readonly ITicketNoteRepository _ticketNoteRepository = ticketNoteRepository ?? throw new ArgumentNullException(nameof(ticketNoteRepository));
		private readonly ILogger<TicketNoteController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

		// delete ticketnote
		[HttpDelete("{noteId}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteTicketNoteAsync(string noteId, CancellationToken cancellationToken)
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

		[HttpPatch("{noteId}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> UpdateTicketNoteAsync(string noteId, [FromBody] UpdatedTicketModel updatedTicketModel, CancellationToken cancellationToken)
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
}
