using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities;

public class Ticket : GuidEntity, IEntity
{
	public required string KioskId { get; set; }
	public TicketStatusType Status { get; set; }
	public DateTime OpenDate { get; set; }
	public DateTime? CloseDate { get; set; }
	public required string OpenedBy { get; set; }
	public string? Description { get; set; }

	public required string Title { get; set; }

	/*		[JsonIgnore]
			public virtual Kiosk Kiosk { get; set; }*/

	public virtual ICollection<TicketNote> TicketNotes { get; set; } = new List<TicketNote>();

	[SetsRequiredMembers]
	protected Ticket() : base()
	{
		OpenDate = DateTime.Now;
		Status = TicketStatusType.Open;
	}

	[SetsRequiredMembers]
	public Ticket(string kioskId, string openedBy, string title, string? description) : this()
	{
		KioskId = kioskId;
		OpenedBy = openedBy;
		Title = title;
		Description = description;
	}
}
