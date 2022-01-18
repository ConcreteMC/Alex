using System.Collections.Generic;
using System.Linq;
using Alex.Blocks.Materials;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class RedstoneTorch : Block
	{
		public bool IsWallTorch { get; }
		public RedstoneTorch(bool wallTorch = false) : base()
		{
			IsWallTorch = wallTorch;
			
			Solid = false;
			Transparent = true;
			Luminance = 7;
			
			BlockMaterial = Material.Decoration;
		}
		
		public override IEnumerable<BoundingBox> GetBoundingBoxes(Vector3 blockPos)
		{
			var min = base.GetBoundingBoxes(blockPos).MinBy(x => x.GetDimensions().LengthSquared());
			min.Inflate(0.25f);
			yield return min;
				
			yield break;
		}
	}
}
