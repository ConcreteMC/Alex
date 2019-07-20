using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models.Blocks
{
	public class BlockModelElementFace
	{
		/// <summary>
		/// Defines the area of the texture to use according to the scheme [x1, y1, x2, y2]. If unset, it defaults to values equal to xyz position of the element. The texture behavior will be inconsistent if UV extends below 0 or above 16. If the numbers of x1 and x2 are swapped (e.g. from 0, 0, 16, 16 to 16, 0, 0, 16), the texture will be flipped. UV is optional, and if not supplied it will automatically generate based on the element's position.
		/// </summary>
		public BlockModelElementFaceUV UV { get; set; } = null;//new BlockModelElementFaceUV(0, 0, 16, 16);

		/// <summary>
		/// Specifies the texture in form of the texture variable prepended with a #.
		/// </summary>
		[JsonProperty("texture")]
		public string Texture { get; set; }

		/// <summary>
		/// Specifies whether a face does not need to be rendered when there is a block touching it in the specified position. The position can be: down, up, north, south, west, or east. It will also determine which side of the block to use the light level from for lighting the face, and if unset, defaults to the side.
		/// </summary>
		public string CullFace { get; set; } = "none";

		/// <summary>
		/// Rotates the texture by the specified number of degrees. Can be 0, 90, 180, or 270. Defaults to 0. Rotation does not affect which part of the texture is used. Instead, it amounts to permutation of the selected texture vertexes (selected implicitly, or explicitly though uv).
		/// </summary>
		public int Rotation { get; set; } = 0;

		/// <summary>
		/// Determines whether to tint the texture using a hardcoded tint index. The default is not using the tint, and any number causes it to use tint. Note that only certain blocks have a tint index, all others will be unaffected.
		/// </summary>
		public int TintIndex { get; set; } = -1;

	}
}
