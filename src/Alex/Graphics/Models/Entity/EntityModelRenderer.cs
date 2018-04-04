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
		private static ConcurrentDictionary<string, VertexPositionNormalTexture[]> ModelBonesCache { get; } = new ConcurrentDictionary<string, VertexPositionNormalTexture[]>();

		private EntityModel Model { get; }
		private IReadOnlyDictionary<int, EntityModelCube> Cubes { get; }
		private Texture2D Texture { get; set; }
		public EntityModelRenderer(EntityModel model, Texture2D texture)
		{
			Model = model;
			Texture = texture;

			var cubes = new Dictionary<int, EntityModelCube>();
			Cache(cubes);

			Cubes = cubes;
		}

		private void Cache(Dictionary<int, EntityModelCube> cubes)
		{
			foreach (var bone in Model.Bones)
			{
				if (bone == null) continue;
				if (bone.NeverRender) continue;

				if (bone.Cubes != null)
				{
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

						VertexPositionNormalTexture[] vertices = ModelBonesCache.GetOrAdd($"{Model.Name}:{bone.Name}", s =>
						{
							Cube built = new Cube(size, new Vector2(Texture.Width, Texture.Height));
							built.BuildCube(cube.Uv);

							return built.Front.Concat(built.Back).Concat(built.Top).Concat(built.Bottom).Concat(built.Left)
								.Concat(built.Right).ToArray();
						});

						var part = new EntityModelCube(vertices,
							Texture,
							rotation, pivot, origin);

						part.Mirror = bone.Mirror;
						if (!bone.Name.Contains("head"))
						{
							part.ApplyPitch = false;
						}

						if (!cubes.TryAdd(bone.GetHashCode(), part))
						{
							part.Dispose();
							Log.Warn($"Failed to add cube to list of bones: {Model.Name}:{bone.Name}");
						}
					}
				}
			}
		}

		public void Render(IRenderArgs args, PlayerLocation position)
		{
			foreach (var bone in Cubes)
			{
				if (bone.Value.IsDirty)
					continue;


				bone.Value.Render(args, position);
			}
		}

		public Vector3 DiffuseColor { get; set; } = Color.White.ToVector3();

		public void Update(IUpdateArgs args, PlayerLocation position)
		{
			foreach (var bone in Cubes)
			{
				if (bone.Value.Effect != null)
				{
					bone.Value.Effect.DiffuseColor = DiffuseColor;
				}

				if (!bone.Value.IsDirty)
					continue;
				
				bone.Value.Update(args);
			}
		}

		public override string ToString()
		{
			return Model.Name;
		}

		private class EntityModelCube : IDisposable
		{
			public AlphaTestEffect Effect { get; private set; }

			private VertexBuffer Buffer { get; set; }
			private VertexPositionNormalTexture[] _vertices;

			public bool IsDirty { get; private set; }
			public Texture2D Texture { get; private set; }

			public Vector3 Rotation { get; private set; } = Vector3.Zero;
			public Vector3 Pivot { get; private set; } = Vector3.Zero;
			public Vector3 Origin { get; private set; }
			public EntityModelCube(VertexPositionNormalTexture[] textures, Texture2D texture, Vector3 rotation, Vector3 pivot, Vector3 origin)
			{
				_vertices = (VertexPositionNormalTexture[]) textures.Clone();
				Texture = texture;
				Rotation = rotation;
				Pivot = pivot;
				Origin = origin;

				Apply(Origin, Pivot, Rotation);

				IsDirty = true;
			}

			public void Update(IUpdateArgs args)
			{
				if (!IsDirty) return;
				var device = args.GraphicsDevice;
				//if (_vertices.Length > 0)
				//	Apply(ref _vertices, Origin, Pivot, Rotation);

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
			public bool Mirror { get; set; } = false;
			public void Render(IRenderArgs args, PlayerLocation position)
			{
				if (_vertices == null || _vertices.Length == 0) return;

				if (Effect == null || Buffer == null) return;

				var buffer = Buffer;

				Effect.World = Matrix.CreateTranslation(-Pivot)
				               * Matrix.CreateFromYawPitchRoll(ApplyYaw ? MathUtils.ToRadians(position.HeadYaw) : 0f,
					               ApplyPitch ? MathUtils.ToRadians(position.Pitch) : 0f, 0f)
				               * Matrix.CreateTranslation(Pivot)
				               * (Matrix.CreateScale(1f / 16f) * Matrix.CreateTranslation(position));

				Effect.View = args.Camera.ViewMatrix;
				Effect.Projection = args.Camera.ProjectionMatrix;

				args.GraphicsDevice.SetVertexBuffer(buffer);
				foreach (var pass in Effect.CurrentTechnique.Passes)
				{
					pass.Apply();
				}

				args.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertices.Length / 3);
			}

			private void Apply(Vector3 origin, Vector3 pivot, Vector3 rotation)
			{
				Matrix transform =  Matrix.CreateTranslation(-pivot) *
					Matrix.CreateRotationX((rotation.X + 360f) % 360f) *
					Matrix.CreateRotationY((rotation.Y + 360f) % 360f) *
					Matrix.CreateRotationZ((rotation.Z + 360f) % 360f) *
					Matrix.CreateTranslation(pivot);

				for (int i = 0; i < _vertices.Length; i++)
				{
					var pos = _vertices[i].Position;

					pos = Vector3.Add(pos, origin );

					if (rotation != Vector3.Zero)
					{
						pos = Vector3.Transform(pos, transform);
					}

					_vertices[i].Position = pos;
				}
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
