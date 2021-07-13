using System.Collections.Generic;
using Alex.Blocks.Materials;
using Alex.Common.Utils.Noise;
using Alex.Common.Utils.Vectors;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class LilyPad : Block
	{
		public LilyPad() : base()
		{
			Solid = true;
			Transparent = true;
			IsFullCube = false;

			BlockMaterial = Material.Plants.Clone().SetTranslucent();
		}

		/// <inheritdoc />
		public override Vector3 GetOffset(IModule3D noise, BlockCoordinates position)
		{
			return new Vector3(0f, -0.2f, 0f);
		}

		/// <inheritdoc />
		public override IEnumerable<BoundingBox> GetBoundingBoxes(Vector3 blockPos)
		{
			yield return new BoundingBox(blockPos, blockPos + new Vector3(1f, 0.1f, 1f));
		}
	}
}
