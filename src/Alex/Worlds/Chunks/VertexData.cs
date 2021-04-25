using System.Runtime.InteropServices;
using Alex.API.Blocks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace Alex.Worlds.Chunks
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct VertexData
	{
		public readonly Vector3 Position;
		
		public readonly BlockFace Face;

		public readonly Short4 TexCoords;

		public readonly uint Color;

		public VertexData(Vector3 position, BlockFace face, Short4 textureCoordinates, uint color)
		{
			Position = position;
			Face = face;
			TexCoords = textureCoordinates;
			Color = color;
		}
	}
}