using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class Salmon : AbstractFish
	{
		/// <inheritdoc />
		public Salmon(World level) : base(EntityType.Salmon, level)
		{
			Width = 0.7;
			Height = 0.4;
		}
	}
}