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
			private VertexBuffer Buffer { get; set; }
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
				args.GraphicsDevice.SetVertexBuffer(Buffer);

				for (var index = 0; index < Parts.Length; index++)
				{
					var part = Parts[index];

					AlphaTestEffect effect = part.Effect;
					if (effect == null) continue;
					
					var yaw = part.ApplyYaw ? MathUtils.ToRadians(180f - position.Yaw) : 0f;
					var headYaw = part.ApplyHeadYaw ? MathUtils.ToRadians(180f - position.HeadYaw) : 0f;
					var pitch = part.ApplyPitch ? MathUtils.ToRadians(position.Pitch) : 0f;

					var rot = _rotation + part.Rotation;

					Matrix rotMatrix = Matrix.CreateTranslation(-part.Pivot) * Matrix.CreateRotationX((rot.X)) *
					                   Matrix.CreateRotationY((rot.Y)) *
					                   Matrix.CreateRotationZ((rot.Z)) * Matrix.CreateTranslation(part.Pivot);

					effect.World = rotMatrix * Matrix.CreateRotationY(yaw) *
					               (Matrix.CreateTranslation(-part.Pivot) * Matrix.CreateFromYawPitchRoll(headYaw, -pitch, 0f) *
					                Matrix.CreateTranslation(part.Pivot))
					               * (Matrix.CreateScale(1f / 16f) * Matrix.CreateTranslation(position));

					//Effect.World = world * (Matrix.CreateScale(1f / 16f) * Matrix.CreateTranslation(position));
					effect.View = args.Camera.ViewMatrix;
					effect.Projection = args.Camera.ProjectionMatrix;

					foreach (var pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
					}

					args.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, index * (Buffer.VertexCount / Parts.Length), part.Vertices.Length / 3);
					//part.Render(args, position, _rotation);
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
				var vertices = Parts.SelectMany(x => x.Vertices).ToArray();

				VertexBuffer currentBuffer = Buffer;
				
				if (vertices.Length > 0 && (Buffer == null || currentBuffer.VertexCount != vertices.Length))
				{
					if (currentBuffer == null)
					{
						Buffer = GpuResourceManager.GetBuffer(device,
							VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
						currentBuffer = Buffer;
						currentBuffer.SetData(vertices);
					}
					else if (vertices.Length > currentBuffer.VertexCount)
					{
						VertexBuffer oldBuffer = currentBuffer;

						currentBuffer = GpuResourceManager.GetBuffer(device, VertexPositionNormalTextureColor.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
						currentBuffer.SetData(vertices);

						Buffer = currentBuffer;
						oldBuffer.Dispose();
					}
					else
					{
						currentBuffer.SetData(vertices);
					}
				}
			}

			public void Dispose()
			{
				Buffer?.Dispose();
			}
		}
	}
}
