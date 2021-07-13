using System;
using Alex.Blocks.Materials;
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

			base.BlockMaterial = Material.Grass;
		}
	}
}
