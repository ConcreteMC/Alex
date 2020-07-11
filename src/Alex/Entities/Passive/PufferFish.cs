using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class PufferFish : AbstractFish
	{
		/// <inheritdoc />
		public PufferFish(World level) : base(EntityType.PufferFish, level)
		{
			Width = Height = 0.7;
		}
	}
}