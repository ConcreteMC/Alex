using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class Cod : AbstractFish
	{
		/// <inheritdoc />
		public Cod(World level) : base(level)
		{
			Width = 0.5; 
			Height = 0.3;
		}
	}
}