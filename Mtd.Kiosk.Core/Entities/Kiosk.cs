using Mtd.Core.Entities;

namespace Mtd.Kiosk.Core.Entities
{
	public class Kiosk : GuidEntity, IEntity
	{
		public required bool Deleted { get; set; }
	}
}
