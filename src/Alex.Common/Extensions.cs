using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Blocks;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Utils.Vectors;
using Alex.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Common
{
	public static class Extensions
	{
		private static Texture2D WhiteTexture { get; set; }

		public static BlockCoordinates GetBlockCoordinates(this BlockFace face)
		{
			return face switch
			{
				BlockFace.Down  => BlockCoordinates.Down,
				BlockFace.Up    => BlockCoordinates.Up,
				BlockFace.East  => BlockCoordinates.East,
				BlockFace.West  => BlockCoordinates.West,
				BlockFace.North => BlockCoordinates.North,
				BlockFace.South => BlockCoordinates.South,
				_               => BlockCoordinates.Zero
			};
		}
		
		public static void Init(GraphicsDevice gd)
		{
			WhiteTexture = new Texture2D(gd, 1, 1);
			WhiteTexture.SetData(new Color[] { Color.White });

			var size = Vector3.One;

			// Calculate the position of the vertices on the top face.
			Vector3 topLeftFront = new Vector3(size.X, size.Y, size.Z);
			Vector3 topLeftBack = new Vector3(0, size.Y, size.Z);
			Vector3 topRightFront = new Vector3(size.X, size.Y, 0);
			Vector3 topRightBack = new Vector3(0, size.Y, 0);

			// Calculate the position of the vertices on the bottom face.
			Vector3 btmLeftFront = new Vector3(size.X, 0, size.Z);
			Vector3 btmLeftBack = new Vector3(0, 0, size.Z);
			Vector3 btmRightFront = new Vector3(size.X, 0, 0);
			Vector3 btmRightBack = Vector3.Zero;

			var x1 = 0;
			var x2 = 1f;
			var y1 = 0;
			var y2 = 1f;

			// UV texture coordinates
			Vector2 textureTopLeft = new Vector2(x1, y1);
			Vector2 textureTopRight = new Vector2(x2, y1);
			Vector2 textureBottomLeft = new Vector2(x1, y2);
			Vector2 textureBottomRight = new Vector2(x2, y2);

			// Add the vertices for the FRONT face.
			CubeVertices[0] = new VertexPositionColorTexture(topLeftFront, Color.White, textureTopLeft);
			CubeVertices[1] = new VertexPositionColorTexture(btmLeftFront, Color.White, textureBottomLeft);
			CubeVertices[2] = new VertexPositionColorTexture(topRightFront, Color.White, textureTopRight);
			CubeVertices[3] = new VertexPositionColorTexture(btmLeftFront, Color.White, textureBottomLeft);
			CubeVertices[4] = new VertexPositionColorTexture(btmRightFront, Color.White, textureBottomRight);
			CubeVertices[5] = new VertexPositionColorTexture(topRightFront, Color.White, textureTopRight);

			// Add the vertices for the BACK face.
			CubeVertices[6] = new VertexPositionColorTexture(topLeftBack, Color.White, textureTopRight);
			CubeVertices[7] = new VertexPositionColorTexture(topRightBack, Color.White, textureTopLeft);
			CubeVertices[8] = new VertexPositionColorTexture(btmLeftBack, Color.White, textureBottomRight);
			CubeVertices[9] = new VertexPositionColorTexture(btmLeftBack, Color.White, textureBottomRight);
			CubeVertices[10] = new VertexPositionColorTexture(topRightBack, Color.White, textureTopLeft);
			CubeVertices[11] = new VertexPositionColorTexture(btmRightBack, Color.White, textureBottomLeft);

			// Add the vertices for the TOP face.
			CubeVertices[12] = new VertexPositionColorTexture(topLeftFront, Color.White, textureBottomLeft);
			CubeVertices[13] = new VertexPositionColorTexture(topRightBack, Color.White, textureTopRight);
			CubeVertices[14] = new VertexPositionColorTexture(topLeftBack, Color.White, textureTopLeft);
			CubeVertices[15] = new VertexPositionColorTexture(topLeftFront, Color.White, textureBottomLeft);
			CubeVertices[16] = new VertexPositionColorTexture(topRightFront, Color.White, textureBottomRight);
			CubeVertices[17] = new VertexPositionColorTexture(topRightBack, Color.White, textureTopRight);

			// Add the vertices for the BOTTOM face. 
			CubeVertices[18] = new VertexPositionColorTexture(btmLeftFront, Color.White, textureTopLeft);
			CubeVertices[19] = new VertexPositionColorTexture(btmLeftBack, Color.White, textureBottomLeft);
			CubeVertices[20] = new VertexPositionColorTexture(btmRightBack, Color.White, textureBottomRight);
			CubeVertices[21] = new VertexPositionColorTexture(btmLeftFront, Color.White, textureTopLeft);
			CubeVertices[22] = new VertexPositionColorTexture(btmRightBack, Color.White, textureBottomRight);
			CubeVertices[23] = new VertexPositionColorTexture(btmRightFront, Color.White, textureTopRight);

			// Add the vertices for the LEFT face.
			CubeVertices[24] = new VertexPositionColorTexture(topLeftFront, Color.White, textureTopRight);
			CubeVertices[25] = new VertexPositionColorTexture(btmLeftBack, Color.White, textureBottomLeft);
			CubeVertices[26] = new VertexPositionColorTexture(btmLeftFront, Color.White, textureBottomRight);
			CubeVertices[27] = new VertexPositionColorTexture(topLeftBack, Color.White, textureTopLeft);
			CubeVertices[28] = new VertexPositionColorTexture(btmLeftBack, Color.White, textureBottomLeft);
			CubeVertices[29] = new VertexPositionColorTexture(topLeftFront, Color.White, textureTopRight);

			// Add the vertices for the RIGHT face. 
			CubeVertices[30] = new VertexPositionColorTexture(topRightFront, Color.White, textureTopLeft);
			CubeVertices[31] = new VertexPositionColorTexture(btmRightFront, Color.White, textureBottomLeft);
			CubeVertices[32] = new VertexPositionColorTexture(btmRightBack, Color.White, textureBottomRight);
			CubeVertices[33] = new VertexPositionColorTexture(topRightBack, Color.White, textureTopRight);
			CubeVertices[34] = new VertexPositionColorTexture(topRightFront, Color.White, textureTopLeft);
			CubeVertices[35] = new VertexPositionColorTexture(btmRightBack, Color.White, textureBottomRight);
		}

		/// <summary>
		/// Draw a line between the two supplied points.
		/// </summary>
		/// <param name="start">Starting point.</param>
		/// <param name="end">End point.</param>
		/// <param name="color">The draw color.</param>
		public static void DrawLine(this SpriteBatch sb,
			float thickness,
			Vector2 start,
			Vector2 end,
			Color color,
			Vector2 scale,
			float layerdepth)
		{
			float length = (end - start).Length();
			float rotation = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

			sb.Draw(
				WhiteTexture, start, null, color, rotation, Vector2.Zero,
				new Vector2(scale.X * length, scale.Y * (thickness)), SpriteEffects.None, layerdepth);
		}

		/// <summary>
		/// Draw a rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle to draw.</param>
		/// <param name="color">The draw color.</param>
		public static void DrawRectangle(this SpriteBatch sb, Rectangle rectangle, Color color)
		{
			sb.Draw(WhiteTexture, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 1), color);
			sb.Draw(WhiteTexture, new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, 1), color);
			sb.Draw(WhiteTexture, new Rectangle(rectangle.Left, rectangle.Top, 1, rectangle.Height), color);
			sb.Draw(WhiteTexture, new Rectangle(rectangle.Right, rectangle.Top, 1, rectangle.Height + 1), color);
		}

		/// <summary>
		/// Fill a rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle to fill.</param>
		/// <param name="color">The fill color.</param>
		public static void FillRectangle(this SpriteBatch sb, Rectangle rectangle, Color color)
		{
			sb.Draw(WhiteTexture, rectangle, color);
		}

		public static void FillRectangle(this SpriteBatch sb, Rectangle rectangle, Color color, float layerDepth)
		{
			sb.Draw(WhiteTexture, rectangle, null, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
		}

		public static void RenderBox(this GraphicsDevice device,
			Vector3[] corners,
			Matrix view,
			Matrix projection,
			Color color)
		{
			var effect = GetEffect(device);

			effect.View = view;
			effect.Projection = projection;

			var oldDiffuse = effect.DiffuseColor;
			var oldWorld = effect.World;
			var oldAlpha = effect.Alpha;


			for (var i = 0; i < 8; i++)
			{
				Verts[i].Position = corners[i];
				Verts[i].Color = color;
			}

			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();

				device.DrawUserIndexedPrimitives(PrimitiveType.LineList, Verts, 0, 8, Indices, 0, Indices.Length / 2);
			}

			effect.DiffuseColor = oldDiffuse;
			effect.World = oldWorld;
			effect.Alpha = oldAlpha;
		}

		private static BasicEffect GetEffect(GraphicsDevice device)
		{
			if (_effect == null)
			{
				_effect = new BasicEffect(device)
				{
					VertexColorEnabled = true,
					FogEnabled = false,
					DiffuseColor = Color.White.ToVector3(),
					LightingEnabled = false
				};
			}

			return _effect;
		}

		public static void RenderBoundingBox(this GraphicsDevice sb,
			BoundingBox box,
			Matrix view,
			Matrix projection,
			Color color,
			bool asCube = false,
			BasicEffect effect = null)
		{
			if (effect == null)
				effect = GetEffect(sb);

			effect.View = view;
			effect.Projection = projection;

			var oldDiffuse = effect.DiffuseColor;
			var oldWorld = effect.World;
			var oldAlpha = effect.Alpha;

			if (asCube)
			{
				for (var index = 0; index < CubeVertices.Length; index++)
				{
					CubeVertices[index].Color = color;
				}

				const float inflation = 0.025f;

				var x = (box.Max.X - box.Min.X) + inflation;
				var y = (box.Max.Y - box.Min.Y) + inflation;
				var z = (box.Max.Z - box.Min.Z) + inflation;

				effect.World = Matrix.CreateScale(new Vector3(x, y, z))
				               * Matrix.CreateTranslation(box.Min - new Vector3(inflation / 2f));

				effect.Alpha = 0.5f;
			}
			else
			{
				var corners = box.GetCorners();

				for (var i = 0; i < 8; i++)
				{
					Verts[i].Position = corners[i];
					Verts[i].Color = color;
				}
			}

			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();

				if (!asCube)
				{
					sb.DrawUserIndexedPrimitives(PrimitiveType.LineList, Verts, 0, 8, Indices, 0, Indices.Length / 2);
				}
				else
				{
					sb.DrawUserPrimitives(
						PrimitiveType.TriangleList, CubeVertices, 0, CubeVertices.Length / 3,
						VertexPositionColorTexture.VertexDeclaration);
				}
			}

			effect.DiffuseColor = oldDiffuse;
			effect.World = oldWorld;
			effect.Alpha = oldAlpha;
		}

		#region Fields

		private static readonly VertexPositionColorTexture[] CubeVertices = new VertexPositionColorTexture[36];

		private static readonly VertexPositionColor[] Verts = new VertexPositionColor[8];

		private static readonly short[] Indices =
		{
			0, 1, 1, 2, 2, 3, 3, 0, 0, 4, 1, 5, 2, 6, 3, 7, 4, 5, 5, 6, 6, 7, 7, 4
		};

		private static BasicEffect _effect;

		#endregion

		public static string GetBytesReadable(long i)
		{
			// Get absolute value
			long absolute_i = (i < 0 ? -i : i);
			// Determine the suffix and readable value
			string suffix;
			double readable;

			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				readable = (i >> 50);
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				readable = (i >> 40);
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				readable = (i >> 30);
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				readable = (i >> 20);
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				readable = (i >> 10);
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				readable = i;
			}
			else
			{
				return i.ToString("0 B"); // Byte
			}

			// Divide by 1024 to get fractional value
			readable = (readable / 1024);

			// Return formatted number with suffix
			return readable.ToString("0.### ") + suffix;
		}
	}
	
	public static class HttpClientExtensions
	{
		private const int BufferSize = 8192;
		
		public struct HttpDownloadProgress
		{
			public ulong BytesReceived { get; set; }

			public ulong? TotalBytesToReceive { get; set; }
		}
		
		public static async Task<byte[]> DownloadDataAsync(this HttpClient client, Uri requestUri, Action<HttpDownloadProgress> progress, CancellationToken cancellationToken)
		{
			if (client == null)
			{
				throw new ArgumentNullException(nameof(client));
			}

			using (var responseMessage = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
			{
				responseMessage.EnsureSuccessStatusCode();

				var content = responseMessage.Content;
				if (content == null)
				{
					return Array.Empty<byte>();
				}

				var headers = content.Headers;
				var contentLength = headers.ContentLength;
				using (var responseStream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
				{
					var buffer = new byte[BufferSize];
					int bytesRead;
					var bytes = new List<byte>();

					var downloadProgress = new HttpDownloadProgress();
					if (contentLength.HasValue)
					{
						downloadProgress.TotalBytesToReceive = (ulong)contentLength.Value;
					}
					progress?.Invoke(downloadProgress);
					//progress?.Report(downloadProgress);

					while ((bytesRead = await responseStream.ReadAsync(buffer, 0, BufferSize, cancellationToken).ConfigureAwait(false)) > 0)
					{
						bytes.AddRange(buffer.Take(bytesRead));

						downloadProgress.BytesReceived += (ulong)bytesRead;
						progress?.Invoke(downloadProgress);
						//progress?.Report(downloadProgress);
					}

					return bytes.ToArray();
				}
			}
		}
	}
}