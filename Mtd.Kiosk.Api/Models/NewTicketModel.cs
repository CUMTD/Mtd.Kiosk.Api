using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Api.Models
{
	public class NewTicketModel
	{
		public required string KioskId { get; set; }
		public required string OpenedBy { get; set; }
		public required string Description { get; set; }

		public Ticket ToTicket() => new Ticket(KioskId, OpenedBy, Description);
	}
}
