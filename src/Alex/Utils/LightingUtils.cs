using Alex.ResourcePackLib.Json;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
	public class LightingUtils
	{
		public static readonly Color LightColor =
			new Color(245, 245, 225);

		/// <summary>
		/// The default lighting information for rendering a block;
		///  i.e. when the lighting param to CreateUniformCube == null.
		/// </summary>
		public static readonly int[] DefaultLighting =
			new int[]
			{
				15, 15, 15,
				15, 15, 15
			};

		/// <summary>
		/// Maps a light level [0..15] to a brightness modifier for lighting.
		/// </summary>
		public static readonly float[] CubeBrightness =
			new float[]
			{
				0.050f, 0.067f, 0.085f, 0.106f, // [ 0..3 ]
				0.129f, 0.156f, 0.186f, 0.221f, // [ 4..7 ]
				0.261f, 0.309f, 0.367f, 0.437f, // [ 8..11]
				0.525f, 0.638f, 0.789f, 1.000f //  [12..15]
			};

		/// <summary>
		/// The per-face brightness modifier for lighting.
		/// </summary>
		public static readonly float[] FaceBrightness =
			new float[]
			{
				0.6f, 0.6f, // North / South
				0.8f, 0.8f, // East / West
				1.0f, 0.5f // Top / Bottom
			};

		public static Color AdjustColor(Color color, BlockFace face, int lighting, bool shade = true)
		{
			float brightness = 1f;
			if (shade)
			{
				switch (face)
				{
					case BlockFace.Down:
						brightness = FaceBrightness[5];
						break;
					case BlockFace.Up:
						brightness = FaceBrightness[4];
						break;
					case BlockFace.East:
						brightness = FaceBrightness[2];
						break;
					case BlockFace.West:
						brightness = FaceBrightness[3];
						break;
					case BlockFace.North:
						brightness = FaceBrightness[0];
						break;
					case BlockFace.South:
						brightness = FaceBrightness[1];
						break;
					case BlockFace.None:

						break;
				}
			}

			var light = LightColor.ToVector3() * CubeBrightness[lighting];
			return new Color(brightness * (color.ToVector3() * light));
		}
	}
}