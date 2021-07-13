using System;
using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class WallSign : Block
	{
		public WoodType WoodType { get; }
		
		public WallSign(WoodType woodType = WoodType.Oak) : base()
		{
			WoodType = woodType;
			
			Solid = false;
			Transparent = true;
			Renderable = false;
			CanInteract = true;
			
			HasHitbox = true;

			//RequiresUpdate = true

			base.BlockMaterial = Material.Wood.Clone().WithMapColor(woodType.ToMapColor());
		}
	}

	public class StandingSign : Block
	{
		public WoodType WoodType { get; }
		public StandingSign(WoodType woodType = WoodType.Oak)
		{
			WoodType = woodType;
			
			Solid = false;
			Transparent = true;
			Renderable = false;
			CanInteract = true;
			
			HasHitbox = true;

			//RequiresUpdate = true;

			BlockMaterial = Material.Wood.Clone().WithMapColor(woodType.ToMapColor());
		}
	}

	public enum WoodType
	{
		Oak,
		Spruce,
		Birch,
		Jungle,
		Acacia,
		DarkOak,
		Crimson,
		Warped
	}
}
