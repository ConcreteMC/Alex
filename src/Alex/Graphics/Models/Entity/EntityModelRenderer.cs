using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Rendering.Camera;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Graphics.Models.Entity
{
	public class EntityModelRenderer : Model
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityModelRenderer));

		private EntityModel Model { get; }
		private IReadOnlyDictionary<string, ModelBone> Bones { get; }
		public Texture2D Texture { get; set; }
		public EntityModelRenderer(EntityModel model, Texture2D texture)
		{
			Model = model;
			Texture = texture;

			if (texture == null)
			{
				Log.Warn($"No texture set for rendererer for {model.Name}!");
				return;
			}

			var cubes = new Dictionary<string, ModelBone>();
			Cache(cubes);

			Bones = cubes;
		}

		private void Cache(Dictionary<string, ModelBone> modelBones)
		{
			foreach (var bone in Model.Bones)
			{
				if (bone == null) continue;
				if (bone.NeverRender) continue;

				if (bone.Cubes != null)
				{
					List<EntityModelCube> c = new List<EntityModelCube>();
					ModelBone modelBone;
					foreach (var cube in bone.Cubes)
					{
						if (cube == null)
						{
							Log.Warn("Cube was null!");
							continue;
						}

						var size = cube.Size;
						var origin = cube.Origin;
						var pivot = bone.Pivot;
						var rotation = bone.Rotation;

						
						VertexPositionNormalTexture[] vertices;
						Cube built = new Cube(bone.Mirror ? -size : size, new Vector2(Texture.Width, Texture.Height));
						built.Mirrored = bone.Mirror;
						built.BuildCube(cube.Uv);

						vertices = built.Front.Concat(built.Back).Concat(built.Top).Concat(built.Bottom).Concat(built.Left)
							.Concat(built.Right).ToArray();

						var part = new EntityModelCube(vertices, Texture, rotation, pivot, origin);

						part.Mirror = bone.Mirror;
						if (bone.Name.Contains("head"))
						{
							part.ApplyHeadYaw = true;
						}
						else
						{
							part.ApplyPitch = false;
							part.ApplyYaw = true;
							part.ApplyHeadYaw = false;
						}

						c.Add(part);
					}

					modelBone = new ModelBone(c.ToArray());
					if (!modelBones.TryAdd(bone.Name, modelBone))
					{
						Log.Warn($"Failed to add bone! {Model.Name}:{bone.Name}");
					}
				}
			}
		}

		public void Render(IRenderArgs args, PlayerLocation position)
		{
			foreach (var bone in Bones)
			{
				bone.Value.Render(args, position);
			}
		}

		public Vector3 DiffuseColor { get; set; } = Color.White.ToVector3();

		public void Update(IUpdateArgs args, PlayerLocation position)
		{
			foreach (var bone in Bones)
			{
				bone.Value.Update(args, position, DiffuseColor);
			}
		}

		public bool GetBone(string name, out ModelBone bone)
		{
			return Bones.TryGetValue(name, out bone);
		}

		public override string ToString()
		{
			return Model.Name;
		}

		public class ModelBone
		{
			public EntityModelCube[] Parts { get; }

			private Vector3 _rotation = Vector3.Zero;
			public Vector3 Rotation
			{
				get { return _rotation; }
				set { _rotation = value; }
			}

			public ModelBone(EntityModelCube[] parts)
			{
				Parts = parts;
			}

			public void Render(IRenderArgs args, PlayerLocation position)
			{
				foreach (var part in Parts)
				{
					part.Render(args, position, _rotation);
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

					part.Update(args);
				}
			}
		}

		public class EntityModelCube : IDisposable
		{
			public AlphaTestEffect Effect { get; private set; }

			private VertexBuffer Buffer { get; set; }
			private VertexPositionNormalTexture[] _vertices;

			public bool IsDirty { get; private set; }
			public Texture2D Texture { get; private set; }

			public Vector3 Rotation { get; set; } = Vector3.Zero;
			public Vector3 Pivot { get; private set; } = Vector3.Zero;
			//public Vector3 Origin { get; private set; }
			public EntityModelCube(VertexPositionNormalTexture[] textures, Texture2D texture, Vector3 rotation, Vector3 pivot, Vector3 origin)
			{
				_vertices = (VertexPositionNormalTexture[]) textures.Clone();
				Texture = texture;
				Rotation = rotation;
				Pivot = pivot;
				//Origin = origin;

				for (int i = 0; i < _vertices.Length; i++)
				{
					_vertices[i].Position += origin;
				}

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

				VertexBuffer currentBuffer = Buffer;
				if (_vertices.Length > 0 && (Buffer == null || currentBuffer.VertexCount != _vertices.Length))
				{
					if (currentBuffer == null)
					{
						Buffer = new VertexBuffer(device,
							VertexPositionNormalTexture.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);
						currentBuffer = Buffer;
						currentBuffer.SetData(_vertices);
					}
					else if (_vertices.Length > currentBuffer.VertexCount)
					{
						VertexBuffer oldBuffer = currentBuffer;

						currentBuffer = new VertexBuffer(device, VertexPositionNormalTextureColor.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);
						currentBuffer.SetData(_vertices);

						Buffer = currentBuffer;
						oldBuffer.Dispose();
					}
					else
					{
						currentBuffer.SetData(_vertices);
					}
				}

				IsDirty = false;
			}

			public bool ApplyPitch { get; set; } = true;
			public bool ApplyYaw { get; set; } = true;
			public bool ApplyHeadYaw { get; set; } = false;
			public bool Mirror { get; set; } = false;
			public void Render(IRenderArgs args, PlayerLocation position, Vector3 rot)
			{
				if (_vertices == null || _vertices.Length == 0) return;

				if (Effect == null || Buffer == null) return;

				var buffer = Buffer;

				var yaw = ApplyYaw ? MathUtils.ToRadians(MathUtils.NormDeg(position.Yaw + 180f)) : 0f;
				var headYaw = ApplyHeadYaw ? MathUtils.ToRadians(MathUtils.NormDeg(position.HeadYaw + 180f)) : 0f;
				var pitch = ApplyPitch ? MathUtils.ToRadians(position.Pitch) : 0f;

				rot += Rotation;

				Matrix rotMatrix = Matrix.CreateTranslation(-Pivot) * Matrix.CreateRotationX((rot.X )) *
				                   Matrix.CreateRotationY((rot.Y)) *
				                   Matrix.CreateRotationZ((rot.Z)) * Matrix.CreateTranslation(Pivot);

				Effect.World = rotMatrix * Matrix.CreateRotationY(yaw) *
								  (Matrix.CreateTranslation(-Pivot) * Matrix.CreateFromYawPitchRoll(headYaw, pitch, 0f) * Matrix.CreateTranslation(Pivot))
							   * (Matrix.CreateScale(1f / 16f) * Matrix.CreateTranslation(position));

				//Effect.World = world * (Matrix.CreateScale(1f / 16f) * Matrix.CreateTranslation(position));
				Effect.View = args.Camera.ViewMatrix;
				Effect.Projection = args.Camera.ProjectionMatrix;

				args.GraphicsDevice.SetVertexBuffer(buffer);
				foreach (var pass in Effect.CurrentTechnique.Passes)
				{
					pass.Apply();
				}

				args.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertices.Length / 3);
			}

			public void Dispose()
			{
				IsDirty = false;
				Effect?.Dispose();
				Buffer?.Dispose();
			}
		}
	}
}
