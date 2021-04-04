using Alex.API.Blocks;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Chunks
{
	public struct VertexData
	{
		public Vector3 Position;
		public BlockFace Face;

		public ushort TexCoords;

		public uint Color;

		public byte BlockLight;

		public byte SkyLight;

		public VertexData(Vector3 position, BlockFace face, ushort textureCoordinates, uint color, byte blockLight, byte skyLight)
		{
			Position = position;
			Face = face;
			TexCoords = textureCoordinates;
			Color = color;
			BlockLight = blockLight;
			SkyLight = skyLight;
		}
	}
}