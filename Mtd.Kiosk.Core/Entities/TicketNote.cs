using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities;

public class TicketNote : GuidEntity, IEntity
{
	public string? MarkdownBody { get; set; }
	public string TicketId { get; set; }
	public DateTime CreatedDate { get; set; }
	public string CreatedBy { get; set; }
	public bool Deleted { get; set; }
	public virtual Ticket Ticket { get; set; }

	[SetsRequiredMembers]
	protected TicketNote() : base()
	{
		TicketId = string.Empty;
		CreatedDate = DateTime.Now;
		CreatedBy = string.Empty;
		Deleted = false;
		Ticket = new Ticket();
	}

	[SetsRequiredMembers]
	public TicketNote(string ticketId, string createdBy, string? markdownBody) : this()
	{
		TicketId = ticketId;
		CreatedBy = createdBy;
		MarkdownBody = markdownBody;
	}
}
