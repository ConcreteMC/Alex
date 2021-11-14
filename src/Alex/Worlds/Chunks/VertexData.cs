using System;
using System.Runtime.InteropServices;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Graphics;
using Alex.Common.Utils.Vectors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace Alex.Worlds.Chunks
{
//	[StructLayout(LayoutKind.Sequential)]
	public class VertexData
	{
		public readonly BlockCoordinates LightPosition;
		public readonly Vector3 Position;
		
		public readonly BlockFace Face;
		
		public readonly Short4 TexCoords;

		public readonly uint Color;
		
		private readonly VertexFlags _flags;

		public bool IsTransparent => (_flags & VertexFlags.Transparent) != 0;
		public bool IsFullCube => (_flags & VertexFlags.FullCube) != 0;
		public bool IsSolid => (_flags & VertexFlags.Solid) != 0;

		public readonly RenderStage Stage;
		public VertexData(Vector3 position,
			BlockFace face,
			Vector4 textureCoordinates,
			uint color,
			VertexFlags flags,
			RenderStage stage, BlockCoordinates lightPosition)
		{
			Position = position;
			Face = face;
			TexCoords = new Short4(textureCoordinates);
			Color = color;
			_flags = flags;
			Stage = stage;
			LightPosition = lightPosition;
		}
	}
}