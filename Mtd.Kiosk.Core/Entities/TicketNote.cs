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

		[SetsRequiredMembers]
		protected TicketNote() : base()
		{
			CreatedDate = DateTime.Now;
		}
	}
}
