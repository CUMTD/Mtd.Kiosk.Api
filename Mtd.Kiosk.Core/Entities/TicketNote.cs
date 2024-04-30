using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities
{

	public class TicketNote : GuidEntity, IEntity
	{
		public string? MarkdownBody { get; set; }
		public required string TicketId { get; set; }
		public required DateTime CreatedDate { get; set; }
		public required string CreatedBy { get; set; }

		public required bool Deleted { get; set; }

		[SetsRequiredMembers]
		public TicketNote() : base()
		{
			CreatedDate = DateTime.Now;
			Deleted = false;
		}

		[SetsRequiredMembers]
		public TicketNote(string ticketId, string markdownBody, string createdBy) : this()
		{
			TicketId = ticketId;
			MarkdownBody = markdownBody;
			CreatedBy = createdBy;

		}
	}
}
