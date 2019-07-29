using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Graphics.Models.Entity
{
	public partial class EntityModelRenderer : Model, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityModelRenderer));

		private EntityModel Model { get; }
		private IReadOnlyDictionary<string, ModelBone> Bones { get; }
		public Texture2D Texture { get; set; }
		private VertexBuffer VertexBuffer { get; set; }
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
			List<EntityModelBone> headBones = new List<EntityModelBone>();

			var headBone =
				Model.Bones.FirstOrDefault(x => x.Name.Contains("head", StringComparison.InvariantCultureIgnoreCase));
			if (headBone != null)
			{
				headBones.Add(headBone);
				foreach (var bone in Model.Bones)
				{
					if (bone == headBone) continue;
					if (bone.Parent.Equals(headBone.Name))
					{
						headBones.Add(bone);
					}
				}

				foreach (var bone in Model.Bones.Where(x =>
					!headBones.Any(hb => hb.Name.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase))))
				{
					if (headBones.Any(x => x.Name.Equals(bone.Name, StringComparison.InvariantCultureIgnoreCase)))
					{
						headBones.Add(bone);
					}
				}
			}
			
			List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();

			foreach (var bone in Model.Bones)
			{
				if (bone == null) continue;
				if (bone.NeverRender) continue;
				bool partOfHead = headBones.Contains(bone);

				if (bone.Cubes != null)
				{
					List<ModelBoneCube> c = new List<ModelBoneCube>();
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

						//VertexPositionNormalTexture[] vertices;
						Cube built = new Cube(size, new Vector2(Texture.Width, Texture.Height));
						built.Mirrored = bone.Mirror;
						built.BuildCube(cube.Uv);

						vertices = ModifyCubeIndexes(vertices, ref built.Front, origin);
						vertices = ModifyCubeIndexes(vertices, ref built.Back, origin);
						vertices = ModifyCubeIndexes(vertices, ref built.Top, origin);
						vertices = ModifyCubeIndexes(vertices, ref built.Bottom, origin);
						vertices = ModifyCubeIndexes(vertices, ref built.Left, origin);
						vertices = ModifyCubeIndexes(vertices, ref built.Right, origin);

						var part = new ModelBoneCube(built.Front.indexes
							.Concat(built.Back.indexes)
							.Concat(built.Top.indexes)
							.Concat(built.Bottom.indexes)
							.Concat(built.Left.indexes)
							.Concat(built.Right.indexes)
							.ToArray(), Texture, rotation, pivot, origin);

						part.Mirror = bone.Mirror;
						if (partOfHead)
						{
							part.ApplyHeadYaw = true;
							part.ApplyYaw = true;
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

			VertexBuffer = GpuResourceManager.GetBuffer(this, Alex.Instance.GraphicsDevice,
				VertexPositionNormalTexture.VertexDeclaration, vertices.Count, BufferUsage.None);
			VertexBuffer.SetData(vertices.ToArray());
		}

		private List<VertexPositionNormalTexture> ModifyCubeIndexes(List<VertexPositionNormalTexture> vertices,
			ref (VertexPositionNormalTexture[] vertices, short[] indexes) data, Vector3 offset)
		{
			var startIndex = (short)vertices.Count;
			foreach (var vertice in data.vertices)
			{
				var vertex = vertice;
				vertex.Position += offset;
				vertices.Add(vertex);
			}
			
			//vertices.AddRange(data.vertices);
			
			for (int i = 0; i < data.indexes.Length; i++)
			{
				data.indexes[i] += startIndex;
			}

			return vertices;
		}

		public void Render(IRenderArgs args, PlayerLocation position)
		{
			args.GraphicsDevice.SetVertexBuffer(VertexBuffer);

			if (Bones == null) return;
			foreach (var bone in Bones)
			{
				bone.Value.Render(args, position);
			}
		}

		public Vector3 DiffuseColor { get; set; } = Color.White.ToVector3();

		public void Update(IUpdateArgs args, PlayerLocation position)
		{
			if (Bones == null) return;
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

		public void Dispose()
		{
			if (Bones != null && Bones.Any())
			{
				foreach (var bone in Bones.ToArray())
				{
					bone.Value.Dispose();
				}
			}
			
			Texture?.Dispose();
			VertexBuffer?.Dispose();
		}
	}
}
