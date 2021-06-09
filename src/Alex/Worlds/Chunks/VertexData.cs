using System;
using System.Runtime.InteropServices;
using Alex.Common.Blocks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace Alex.Worlds.Chunks
{
//	[StructLayout(LayoutKind.Sequential)]
	public class VertexData
	{
		public readonly Vector3 Position;
		
		public readonly BlockFace Face;
		
		public readonly Short4 TexCoords;

		public readonly uint Color;

		public readonly byte Flags;

		public bool IsTransparent => (Flags & (byte)(1 << 1)) != 0;
		public bool IsFullCube => (Flags & (byte)(1 << 2)) != 0;
		public bool IsSolid => (Flags & (byte)(1 << 3)) != 0;

		public VertexData(Vector3 position,
			BlockFace face,
			Vector4 textureCoordinates,
			uint color,
			bool isTransparent = false,
			bool isFullCube = false,
			bool isSolid = false)
		{
			Position = position;
			Face = face;
			TexCoords = new Short4(textureCoordinates);
			Color = color;
			Flags = 0;

			if (isTransparent)
				Flags |= (1 << 1);
			
			if (isFullCube)
				Flags |= (1 << 2);

			if (isSolid)
				Flags |= (1 << 3);
		}
	}
}