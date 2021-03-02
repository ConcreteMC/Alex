using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Ghast : Flying
	{
		public Ghast(World level) : base(level)
		{
			Height = 4;
			Width = 4;
		}
	}
}
