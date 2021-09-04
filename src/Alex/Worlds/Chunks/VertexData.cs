using System;
using System.Runtime.InteropServices;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace Alex.Worlds.Chunks
{
//	[StructLayout(LayoutKind.Sequential)]
	public struct VertexData
	{
		public readonly Vector3 Position;
		
		public readonly BlockFace Face;
		
		public readonly Short4 TexCoords;

		public readonly uint Color;
		
		private readonly VertexFlags _flags;

		public bool IsTransparent => (_flags & VertexFlags.Transparent) != 0;
		public bool IsFullCube => (_flags & VertexFlags.FullCube) != 0;
		public bool IsSolid => (_flags & VertexFlags.Solid) != 0;
		public int Index;

		public readonly RenderStage Stage;
		public VertexData(Vector3 position,
			BlockFace face,
			Vector4 textureCoordinates,
			uint color,
			VertexFlags flags,
			RenderStage stage)
		{
			Position = position;
			Face = face;
			TexCoords = new Short4(textureCoordinates);
			Color = color;
			_flags = flags;
			Stage = stage;
			Index = -1;
		}
	}
}