using Mtd.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Mtd.Kiosk.Core.Entities
{
	public class Kiosk : GuidEntity, IEntity
	{
		public required bool Deleted { get; set; }
		// dont want to auto include tickets
		public virtual ICollection<Ticket> Tickets { get; set; }

		[SetsRequiredMembers]

		protected Kiosk()
		{
			Deleted = false;
			Tickets = new List<Ticket>();
		}

		[SetsRequiredMembers]

		public Kiosk(string guid) : this()
		{
			Id = guid;
		}
	}
}
