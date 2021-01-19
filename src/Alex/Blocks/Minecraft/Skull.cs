using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class Skull : Block
	{
		public SkullType SkullType { get; set; } = SkullType.Player;
		public Skull()
		{
			Renderable = false;
			HasHitbox = true;
			
			
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}

		/// <inheritdoc />
		public override IEnumerable<BoundingBox> GetBoundingBoxes(Vector3 blockPos)
		{
			yield return new BoundingBox(blockPos, blockPos + Vector3.One);
		}
	}

	public class WallSkull : Skull
	{
		
	}

	public enum SkullType
	{
		Player,
		Skeleton,
		WitherSkeleton,
		Zombie,
		Creeper,
		Dragon
	}
}