using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities;

[method: SetsRequiredMembers]
public class Ticket() : GuidEntity(), IEntity
{
	public string KioskId { get; set; } = string.Empty;
	public TicketStatusType Status { get; set; } = TicketStatusType.Open;
	public DateTime OpenDate { get; set; } = DateTime.Now;
	public DateTime? CloseDate { get; set; }
	public string OpenedBy { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string Title { get; set; } = string.Empty;
	public virtual Kiosk Kiosk { get; set; } = new Kiosk();
	public virtual ICollection<TicketNote> Notes { get; set; } = [];

	[SetsRequiredMembers]
	public Ticket(string kioskId, string openedBy, string title, string? description) : this()
	{
		KioskId = kioskId;
		OpenedBy = openedBy;
		Title = title;
		Description = description;
	}
}
