using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class PufferFish : AbstractFish
	{
		/// <inheritdoc />
		public PufferFish(World level) : base(EntityType.Pufferfish, level)
		{
			Width = Height = 0.7;
		}
	}
}