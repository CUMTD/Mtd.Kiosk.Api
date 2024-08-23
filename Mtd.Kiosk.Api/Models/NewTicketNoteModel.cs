using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Api.Models;

public class NewTicketNoteModel
{
	public string? MarkdownBody { get; set; }
	public required string CreatedBy { get; set; }
	public TicketNote ToTicketNote(string ticketId) => new(ticketId, CreatedBy, MarkdownBody);
}
