using System;
using Alex.API.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Entity
{
	public partial class EntityModelRenderer
	{
		public class ModelBoneCube : IDisposable
		{
			public AlphaTestEffect Effect { get; private set; }
			//public VertexPositionNormalTexture[] Vertices { get; private set; }
			public short[] Indexes { get; private set; }
			
			public bool IsDirty { get; private set; }
			public Texture2D Texture { get; private set; }

			public Vector3 Rotation { get; set; } = Vector3.Zero;
			public Vector3 Pivot { get; private set; } = Vector3.Zero;
			public Vector3 Origin { get; private set; } = Vector3.Zero;
			public ModelBoneCube(short[] indexes, Texture2D texture, Vector3 rotation, Vector3 pivot,
				Vector3 origin)
			{
				//Vertices = (VertexPositionNormalTexture[]) textures.Clone();
				Texture = texture;
				Rotation = rotation;
				Pivot = pivot;
				Indexes = indexes;
				Origin = origin;
				//for (int i = 0; i < Vertices.Length; i++)
				//{
				//	Vertices[i].Position += origin;
				//}

				IsDirty = true;
			}

			public void Update(IUpdateArgs args)
			{
				if (!IsDirty) return;
				var device = args.GraphicsDevice;

				if (Effect == null)
				{
					Effect = new AlphaTestEffect(device);
					Effect.Texture = Texture;
				}

				IsDirty = false;
			}

			public bool ApplyPitch { get; set; } = true;
			public bool ApplyYaw { get; set; } = true;
			public bool ApplyHeadYaw { get; set; } = false;
			public bool Mirror { get; set; } = false;
			public void Dispose()
			{
				IsDirty = false;
				Effect?.Dispose();
				//Vertices = null;
				//	Buffer?.Dispose();
			}
		}
	}
}
