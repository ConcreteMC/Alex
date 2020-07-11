using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Dolphin : WaterMob
	{
		/// <inheritdoc />
		public Dolphin(World level) : base(EntityType.Dolphin, level)
		{
			Width = 0.9;
			Height = 0.6;
		}
	}
}