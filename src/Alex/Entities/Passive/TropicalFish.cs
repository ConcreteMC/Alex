using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class TropicalFish : AbstractFish
	{
		/// <inheritdoc />
		public TropicalFish(World level) : base(level)
		{
			Width = 0.5;
			Height = 0.4;
		}
	}
}