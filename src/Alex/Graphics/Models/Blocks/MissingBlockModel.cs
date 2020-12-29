using System.Collections.Generic;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
	public class MissingBlockModel : BlockModel
	{
		/// <inheritdoc />
		public override IEnumerable<BoundingBox> GetBoundingBoxes(Vector3 blockPos)
		{
			return new BoundingBox[] {new BoundingBox(blockPos, blockPos + Vector3.One)};
		}
	}
}