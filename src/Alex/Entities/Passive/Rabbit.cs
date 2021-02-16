using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class Rabbit : PassiveMob
	{
		public Rabbit(World level) : base((EntityType)18, level)
		{
			Height = 0.5;
			Width = 0.4;
		}
	}
}
