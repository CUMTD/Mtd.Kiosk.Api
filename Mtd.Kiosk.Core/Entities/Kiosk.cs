using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Core.Entities;

[method: SetsRequiredMembers]
public class Kiosk() : GuidEntity, IEntity
{
	public bool Deleted { get; set; } = false;
	// dont want to auto include tickets

	[JsonIgnore]
	public virtual ICollection<Ticket> Tickets { get; set; } = [];

	[SetsRequiredMembers]

	public Kiosk(string guid) : this()
	{
		Id = guid;
	}
}
