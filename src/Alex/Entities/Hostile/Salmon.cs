using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Salmon : HostileMob
	{
		/// <inheritdoc />
		public Salmon(World level) : base(EntityType.Salmon, level)
		{
			Width = 0.7;
			Height = 0.4;
		}
	}
}