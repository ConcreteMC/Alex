using Microsoft.Xna.Framework;

namespace Alex.Worlds.Chunks
{
	public struct VertexData
	{
		public Vector3 Position;

		public ushort TexCoords;

		public uint Color;

		public byte BlockLight;

		public byte SkyLight;

		public VertexData(Vector3 position, ushort textureCoordinates, uint color, byte blockLight, byte skyLight)
		{
			Position = position;
			TexCoords = textureCoordinates;
			Color = color;
			BlockLight = blockLight;
			SkyLight = skyLight;
		}
	}
}