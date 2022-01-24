using System.Collections.Generic;
using Alex.Blocks.State;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
	public class MissingBlockModel : BlockModel
	{
		/// <inheritdoc />
		public override IEnumerable<BoundingBox> GetBoundingBoxes(BlockState bs, Vector3 blockPos)
		{
			return new BoundingBox[] { new BoundingBox(blockPos, blockPos + Vector3.One) };
		}
	}
}