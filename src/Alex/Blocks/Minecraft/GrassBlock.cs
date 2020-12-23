using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class GrassBlock : Block
	{
		private static readonly PropertyBool Snowy = new PropertyBool("snowy");
		public GrassBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			//IsFullBlock = true;
			//IsFullCube = true;
			//

			//BlockMaterial = Material.Grass;
			Hardness = 0.6f;
		}

		/// <inheritdoc />
		public override IMaterial BlockMaterial
		{
			get
			{
				if (BlockState.GetTypedValue(Snowy))
				{
					return Material.Grass.Clone().SetTintType(TintType.Color, Color.Snow);
				}
				else
				{
					return Material.Grass;
				}
			}
		}
	}
}
