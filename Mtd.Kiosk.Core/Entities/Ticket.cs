using Mtd.Core.Entities;

namespace Mtd.Kiosk.Core.Entities
{
	public class Ticket : GuidEntity, IEntity
	{
		public required string KioskId { get; set; }
		public required TicketStatusType Status { get; set; }
		public required DateTime OpenDate { get; set; }
		public required DateTime? CloseDate { get; set; }
		public required string OpenedBy { get; set; }
		public required string Description { get; set; }
	}
}
