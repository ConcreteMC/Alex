using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class TropicalFish : AbstractFish
	{
		/// <inheritdoc />
		public TropicalFish(World level) : base(EntityType.TropicalFish, level)
		{
			Width = 0.5;
			Height = 0.4;
		}
	}
}