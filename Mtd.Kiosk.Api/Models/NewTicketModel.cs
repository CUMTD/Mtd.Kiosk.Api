using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Api.Models
{
	public class NewTicketModel
	{
		public required string KioskId { get; set; }
		public required string OpenedBy { get; set; }
		public string? Description { get; set; }

		public required string Title { get; set; }

		public Ticket ToTicket() => new(KioskId, OpenedBy, Title, Description);
	}
}
