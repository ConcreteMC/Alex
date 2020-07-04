using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Salmon : HostileMob
	{
		/// <inheritdoc />
		public Salmon(World level) : base(EntityType.Salmon, level) { }
	}
}