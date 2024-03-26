using Mtd.Core.Entities;

namespace Mtd.Kiosk.Core.Entities
{
	public class Person : GuidEntity, IEntity
	{
		public required string First { get; set; }
		public required string Last { get; set; }
	}
}
