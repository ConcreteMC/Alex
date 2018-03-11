using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.Rendering.Camera;
using Alex.ResourcePackLib.Json.Models;
using Alex.Utils;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Entity
{
	public class EntityModelRenderer : Model
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EntityModelRenderer));
		private EntityModel Model { get; }
		private IReadOnlyDictionary<string, ModelPart> Cubes { get; }
		private Texture2D Texture { get; set; }
		//private AlphaTestEffect Effect { get; set; }
		public EntityModelRenderer(EntityModel model, Texture2D texture)
		{
			Model = model;
			Texture = texture;

			var cubes = new Dictionary<string, ModelPart>();
			Cache(cubes);

			Cubes = cubes;
		}

		private class ModelPart : IDisposable
		{
			private AlphaTestEffect Effect { get; set; }
			private VertexBuffer Buffer { get; set; }
			private VertexPositionNormalTexture[] _vertices;

			public bool IsDirty { get; private set; }
			public Texture2D Texture { get; private set; }

			public Vector3 Rotation { get; private set; } = Vector3.Zero;
			public Vector3 Pivot { get; private set; } = Vector3.Zero;
			public Vector3 Origin { get; private set; }
			public ModelPart(VertexPositionNormalTexture[] textures, Texture2D texture, Vector3 rotation, Vector3 pivot, Vector3 origin)
			{
				_vertices = textures;
				Texture = texture;
				Rotation = rotation;
				Pivot = pivot;
				Origin = origin;

				IsDirty = true;
			}

			public void Update(GraphicsDevice device, GameTime gameTime)
			{
				if (!IsDirty) return;

				if (_vertices.Length > 0)
					Mod(ref _vertices, Origin, Pivot, Rotation);

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

			public void Render(IRenderArgs args, Camera camera, Vector3 position, float yaw, float pitch)
			{
				if (_vertices == null || _vertices.Length == 0) return;

				if (Effect == null || Buffer == null) return;

				//Effect.World = Matrix.CreateScale(1f / 16f) * Matrix.CreateTranslation(position);
				Effect.World = Matrix.CreateScale(1f / 16f) * Matrix.CreateRotationY(MathUtils.ToRadians(yaw)) * Matrix.CreateRotationX(MathUtils.ToRadians(pitch)) *
				               Matrix.CreateTranslation(position);

				Effect.View = camera.ViewMatrix;
				Effect.Projection = camera.ProjectionMatrix;

				args.GraphicsDevice.SetVertexBuffer(Buffer);
				foreach (var pass in Effect.CurrentTechnique.Passes)
				{
					pass.Apply();
				}

				args.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertices.Length / 3);
			}

			private void Mod(ref VertexPositionNormalTexture[] data, Vector3 origin, Vector3 pivot, Vector3 rotation)
			{
				Matrix transform =
					Matrix.CreateRotationX(rotation.X) *
					Matrix.CreateRotationY(rotation.Y) *
					Matrix.CreateRotationZ(rotation.Z) *
					Matrix.CreateTranslation(pivot.X, pivot.Y, pivot.Z);

				for (int i = 0; i < data.Length; i++)
				{
					var pos = data[i].Position;

					pos = new Vector3(origin.X + pos.X, origin.Y + pos.Y, origin.Z + pos.Z);
					if (rotation != Vector3.Zero)
					{
						pos = Vector3.Transform(pos, transform);
					}

					data[i].Position = pos;
				}
			}

			public void Dispose()
			{
				Effect?.Dispose();
			}
		}

		private void Cache(Dictionary<string, ModelPart> cubes)
		{
			float x = 0, y = 0, z = 0;
		//	List<VertexPositionNormalTexture> textures = new List<VertexPositionNormalTexture>();
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

						if (cube.Uv == null)
						{
							Log.Warn("Cube.UV was null!");
							continue;
						}

						if (cube.Origin == null)
						{
							Log.Warn("Cube.Origin was null!");
							continue;
						}

						if (cube.Size == null)
						{
							Log.Warn("Cube.Size was null!");
							continue;
						}

						var size = new Vector3(cube.Size.X, cube.Size.Y, cube.Size.Z);
						var origin = new Vector3(cube.Origin.X, cube.Origin.Y, cube.Origin.Z);
						var pivot = Vector3.Zero;
						var rotation = Vector3.Zero;

						if (bone.Pivot != null)
						{
							pivot = new Vector3(bone.Pivot.X, bone.Pivot.Y, bone.Pivot.Z);
						}

						if (bone.Rotation != null)
						{
							rotation = new Vector3(bone.Rotation.X, bone.Rotation.Y, bone.Rotation.Z);
						}

						Cube built = new Cube(size, new Vector2(Texture.Width, Texture.Height));
						built.BuildCube(new Vector2(cube.Uv.X, cube.Uv.Y));
						var combined = built.Front.Concat(built.Back).Concat(built.Top).Concat(built.Bottom).Concat(built.Left)
							.Concat(built.Right).ToArray();

						if (!cubes.TryAdd(bone.Name, new ModelPart(combined, 
							Texture,
							rotation, pivot, origin)))
						{
							Log.Warn($"Failed to add cube to list of bones!");
						}
					}
				}
			}
		}

		public void Render(IRenderArgs args, Camera camera, Vector3 position, float yaw, float pitch)
		{
			foreach (var bone in Cubes)
			{
				if (bone.Value.IsDirty)
					continue;

				bone.Value.Render(args, camera, position, yaw, pitch);
			}
		}

		public void Update(GraphicsDevice device, GameTime gameTime, Vector3 position, float yaw, float pitch)
		{
			foreach (var bone in Cubes)
			{
				if (!bone.Value.IsDirty)
					continue;
				
				bone.Value.Update(device, gameTime);
			}
		}

		public override string ToString()
		{
			return Model.Name;
		}
	}
}
