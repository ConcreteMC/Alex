using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Cod : AbstractFish
	{
		/// <inheritdoc />
		public Cod(World level) : base(EntityType.Cod, level)
		{
			Width = 0.5; 
			Height = 0.3;
		}
	}
}