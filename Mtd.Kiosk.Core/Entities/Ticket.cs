using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities
{
	public class Ticket : GuidEntity, IEntity
	{
		public required string KioskId { get; set; }
		public TicketStatusType Status { get; set; }
		public DateTime OpenDate { get; set; }
		public DateTime? CloseDate { get; set; }
		public required string OpenedBy { get; set; }
		public required string Description { get; set; }

		/*		[JsonIgnore]
				public virtual Kiosk Kiosk { get; set; }*/

		[SetsRequiredMembers]
		protected Ticket() : base()
		{
			OpenDate = DateTime.Now;
			Status = TicketStatusType.Open;
		}

		[SetsRequiredMembers]
		public Ticket(string kioskId, string openedBy, string description) : this()
		{
			KioskId = kioskId;
			OpenedBy = openedBy;
			Description = description;
		}
	}
}
