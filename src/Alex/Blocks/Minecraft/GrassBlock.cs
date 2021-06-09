using System;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class GrassBlock : Block
	{
		private static readonly PropertyBool Snowy = new PropertyBool("snowy", "true", "false");
		public GrassBlock() : base()
		{
			Solid = true;
			Transparent = false;
			//IsFullBlock = true;
			//IsFullCube = true;
			//

			//BlockMaterial = Material.Grass;
			//BlockMaterial = Material.Grass.Clone().SetHardness()
		}

		/// <inheritdoc />
		public override IMaterial BlockMaterial
		{
			get
			{
				if (BlockState.TryGetValue("snowy", out string val) && val.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					return Material.Snow.Clone().SetHardness( 0.6f);//.Clone().SetTintType(TintType.Color, Color.Snow);
				}
				else
				{
					return Material.Grass.Clone().SetHardness( 0.6f);
				}
			}
		}
	}
}
