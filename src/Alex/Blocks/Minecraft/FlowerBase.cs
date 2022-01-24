using Alex.Common.Utils.Noise;
using Alex.Common.Utils.Vectors;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class FlowerBase : Block
	{
		protected FlowerBase()
		{
			IsFullCube = false;
		}

		/// <inheritdoc />
		public override Vector3 GetOffset(IModule3D noise, BlockCoordinates position)
		{
			var noise1 = noise.GetValue(position.X, position.Y, position.Z);
			//	var noise2 = MathF.Abs(noise.GetValue(-position.X, -position.Y, position.Z));
			var noise3 = noise.GetValue(-position.X, -position.Y, -position.Z);

			return new Vector3(noise1 * 0.25f, 0, noise3 * 0.25f);
		}
	}
}