using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class Snowman : PassiveMob
	{
		public Snowman(World level) : base(EntityType.SnowGolem, level)
		{
			Height = 1.9;
			Width = 0.7;
		}
	}
}
