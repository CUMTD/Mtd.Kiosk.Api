using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities;

public class Ticket : GuidEntity, IEntity
{
	public string KioskId { get; set; }
	public TicketStatusType Status { get; set; }
	public DateTime OpenDate { get; set; }
	public DateTime? CloseDate { get; set; }
	public string OpenedBy { get; set; }
	public string? Description { get; set; }
	public string Title { get; set; }

	public virtual ICollection<TicketNote> TicketNotes { get; set; } = [];

	[SetsRequiredMembers]
	protected Ticket() : base()
	{
		KioskId = string.Empty;
		OpenDate = DateTime.Now;
		OpenedBy = string.Empty;
		Title = string.Empty;
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
