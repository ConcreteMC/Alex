using System;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Entity
{
	public partial class EntityModelRenderer
	{
		public class ModelBone : IDisposable
		{
			private IndexBuffer Buffer { get; set; }
			public ModelBoneCube[] Parts { get; }

			private Vector3 _rotation = Vector3.Zero;
			public Vector3 Rotation
			{
				get { return _rotation; }
				set { _rotation = value; }
			}

			public ModelBone(ModelBoneCube[] parts)
			{
				Parts = parts;
			}

			private bool _isDirty = true;
			public void Render(IRenderArgs args, PlayerLocation position)
			{
				if (Buffer == null)
					return;
				
				args.GraphicsDevice.Indices = Buffer;

				int idx = 0;
				for (var index = 0; index < Parts.Length; index++)
				{
					var part = Parts[index];

					AlphaTestEffect effect = part.Effect;
					if (effect == null) continue;
					
					var yaw = part.ApplyYaw ? MathUtils.ToRadians(180f - position.Yaw) : 0f;
					
					var headYaw = part.ApplyHeadYaw ? MathUtils.ToRadians(-(position.HeadYaw - position.Yaw)) : 0f;
					var pitch = part.ApplyPitch ? MathUtils.ToRadians(position.Pitch) : 0f;

					var rot = _rotation + part.Rotation;

					Matrix rotMatrix = Matrix.CreateTranslation(-part.Pivot)
									   * Matrix.CreateFromYawPitchRoll(
																	   MathUtils.ToRadians(rot.Y),
																	   MathUtils.ToRadians(rot.X),
																	   MathUtils.ToRadians(rot.Z)
																	  );
					rotMatrix *= Matrix.CreateTranslation(part.Pivot);
					
					if (part.ApplyYaw)
						rotMatrix *= Matrix.CreateRotationY(yaw);


					var rotMatrix2 = Matrix.CreateTranslation(-part.Pivot) *
						Matrix.CreateFromYawPitchRoll(headYaw, pitch, 0f) *
					                 Matrix.CreateTranslation(part.Pivot);
					
					effect.World =  (rotMatrix2 *
					               rotMatrix 
					              ) * (Matrix.CreateScale(1f / 16f) * Matrix.CreateTranslation(position));

					//Effect.World = world * (Matrix.CreateScale(1f / 16f) * Matrix.CreateTranslation(position));
					effect.View = args.Camera.ViewMatrix;
					effect.Projection = args.Camera.ProjectionMatrix;

					foreach (var pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
					}
					
					args.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, idx, part.Indexes.Length / 3);
					idx += part.Indexes.Length;
				}
			}

			public void Update(IUpdateArgs args, PlayerLocation position, Vector3 diffuseColor)
			{
				foreach (var part in Parts)
				{
					if (part.Effect != null)
					{
						part.Effect.DiffuseColor = diffuseColor;
					}

					if (!part.IsDirty)
						continue;

					_isDirty = true;
					part.Update(args);
				}

				if (_isDirty)
				{
					UpdateVertexBuffer(args.GraphicsDevice);
				}
			}

			private void UpdateVertexBuffer(GraphicsDevice device)
			{
				var indices = Parts.SelectMany(x => x.Indexes).ToArray();

				IndexBuffer currentBuffer = Buffer;

				if (indices.Length > 0 && (Buffer == null || currentBuffer.IndexCount != indices.Length))
				{
					IndexBuffer buffer = GpuResourceManager.GetIndexBuffer(this, device, IndexElementSize.SixteenBits,
						indices.Length, BufferUsage.None);

					buffer.SetData(indices);
					Buffer = buffer;
					
					currentBuffer?.Dispose();
				}
			}

			public void Dispose()
			{
				Buffer?.Dispose();
			}
		}
	}
}
