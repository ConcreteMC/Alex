using System;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Common.Utils
{
	public static class ResourceExtensions
	{
		public static long MemoryUsage(this GraphicsResource resource)
		{
			if (resource is VertexBuffer vb)
			{
				return vb.MemoryUsage();
			}
			else if (resource is IndexBuffer ib)
			{
				return ib.MemoryUsage();
			}
			else if (resource is Texture2D texture)
			{
				return texture.MemoryUsage();
			}

			return 0;
		}

		public static long MemoryUsage(this VertexBuffer buffer)
		{
			return buffer?.VertexDeclaration?.VertexStride * buffer?.VertexCount ?? 0;
		}

		public static long MemoryUsage(this IndexBuffer buffer)
		{
			return (buffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4) * buffer.IndexCount;
		}

		public static long MemoryUsage(this Texture2D texture)
		{
			return texture.Format.GetFormatSize() * texture.Width * texture.Height * texture.LevelCount;
		}

		public static int GetFormatSize(this SurfaceFormat format)
		{
			switch (format)
			{
				case SurfaceFormat.Dxt1:
					return 8;

				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
					return 16;

				case SurfaceFormat.Alpha8:
					return 1;

				case SurfaceFormat.Bgr565:
				case SurfaceFormat.Bgra4444:
				case SurfaceFormat.Bgra5551:
				case SurfaceFormat.HalfSingle:
				case SurfaceFormat.NormalizedByte2:
					return 2;

				case SurfaceFormat.Color:
				case SurfaceFormat.Single:
				case SurfaceFormat.Rg32:
				case SurfaceFormat.HalfVector2:
				case SurfaceFormat.NormalizedByte4:
				case SurfaceFormat.Rgba1010102:
				case SurfaceFormat.Bgra32:
					return 4;

				case SurfaceFormat.HalfVector4:
				case SurfaceFormat.Rgba64:
				case SurfaceFormat.Vector2:
					return 8;

				case SurfaceFormat.Vector4:
					return 16;

				default:
					throw new ArgumentException("Should be a value defined in SurfaceFormat", "Format");
			}
		}
	}
}