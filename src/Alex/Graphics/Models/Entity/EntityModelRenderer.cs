using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Graphics.Models.Entity.Geometry;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using NLog;

namespace Alex.Graphics.Models.Entity
{
	public partial class EntityModelRenderer : Model, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityModelRenderer));

		//private EntityModel Model { get; }
		private IReadOnlyDictionary<string, ModelBone> Bones { get; }
		public PooledTexture2D Texture { get; set; }
		private PooledVertexBuffer VertexBuffer { get; set; }
		public bool Valid { get; private set; }

		public long Vertices => VertexBuffer.VertexCount;
		//public float Height { get; private set; } = 0f;
		public EntityModelRenderer(EntityModel model, PooledTexture2D texture)
		{
			
		//	Model = model;
			Texture = texture;

			if (texture == null)
			{
				Log.Warn($"No texture set for rendererer for {model.Name}!");
				return;
			}

			if (model != null)
			{
				var cubes = new Dictionary<string, ModelBone>();
				Cache(model, cubes);

				Bones = cubes;

				Valid = true;
			}
		}

		public EntityModelRenderer(MinecraftGeometry geometry, PooledTexture2D texture)
		{
			Texture = texture;
			
			var cubes = new Dictionary<string, ModelBone>();

			var converted = geometry.Convert();
			if (converted != null)
			{
				Cache(converted, cubes);
			}
			else
			{
				Cache(geometry, cubes);
			}

			Bones = cubes;
		}

		private void Cache(MinecraftGeometry model, Dictionary<string, ModelBone> modelBones)
		{
			List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
			
			foreach (var bone in model.Bones)
			{
				if (bone == null)
				{
					Log.Warn("Bone null");
					continue;
				}
				
				if (bone.NeverRender) continue;
				
				bool partOfHead = false;

				//bone.Pivot = new Vector3(-bone.Pivot.X, bone.Pivot.Y, bone.Pivot.Z);
				List<ModelBoneCube> c = new List<ModelBoneCube>();
				ModelBone modelBone;
				
				if (bone.PolyMesh != null && bone.PolyMesh.Positions != null)
				{
					var positions = bone.PolyMesh.Positions;
					var uvs = bone.PolyMesh.Uvs;
					var normals = bone.PolyMesh.Normals;
					var polys = bone.PolyMesh.Polys;

					//var verts = new VertexPositionNormalTexture[positions.Length];
				/*	short[] indexes = new short[positions.Length];
					for (int i = 0; i < bone.PolyMesh.Positions.Length; i++)
					{
						vertices.Add(new VertexPositionNormalTexture(positions[i], normals[i], uvs[i]));
						indexes[i] = (short) ((short)startIndex + i);
					}
					*/

				List<short> indices = new List<short>();
				for (int i = 0; i < polys.Length; i++)
				{
					int startIndex = vertices.Count - 1;

					int added = 0;
					var poly = polys[i];
					foreach (var p in poly)
					{

						var pos = positions[p[0]];
						var normal = normals[p[1]];
						var uv = uvs[p[2]];

						vertices.Add(new VertexPositionNormalTexture(pos, normal, uv));
						added++;
						//indices.Add((short) (vertices.Count - 1));
					}

					/*int target = added == 4 ? 6 : 8;
					
					for (int indiceCounter = 0; indiceCounter < added; indiceCounter++)
					{
						//if (indiceCounter < added)
						//{
							indices.Add((short) (startIndex + indiceCounter));
						//}
						//else
						//{
							
						//}
					}*/
					indices.Add( (short) startIndex);
					indices.Add( (short) (startIndex + 1));
					indices.Add((short) (startIndex + 2));
					indices.Add( (short) startIndex);
					indices.Add((short) (startIndex + 3));
					indices.Add((short) (startIndex + 2));
				}
					
					//(VertexPositionNormalTexture[] vertices, short[] indexes) a = (verts, indexes);
					
					var part = new ModelBoneCube(indices.ToArray(), Texture, bone.Rotation, bone.Pivot, Vector3.Zero);

					part.Mirror = false;
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

				modelBone = new ModelBone(c.ToArray(), bone.Parent);
				modelBone.UpdateRotationMatrix = true;
					if (!modelBones.TryAdd(bone.Name, modelBone))
					{
						Log.Debug($"Failed to add bone! {model.Description.Identifier}:{bone.Name}");
					}
				
			}

			if (vertices.Count == 0)
			{
				Log.Warn($"No vertices. {JsonConvert.SerializeObject(model,Formatting.Indented)}");
				Valid = false;
				return;
			}

			VertexBuffer = GpuResourceManager.GetBuffer(this, Alex.Instance.GraphicsDevice,
				VertexPositionNormalTexture.VertexDeclaration, vertices.Count, BufferUsage.None);
			VertexBuffer.SetData(vertices.ToArray());

			Valid = true;
		}

		private void Cache(EntityModel model, Dictionary<string, ModelBone> modelBones)
		{
			List<EntityModelBone> headBones = new List<EntityModelBone>();

			var headBone =
				model.Bones.FirstOrDefault(x => x.Name.Contains("head", StringComparison.InvariantCultureIgnoreCase));
			if (headBone != null)
			{
				headBones.Add(headBone);
				foreach (var bone in model.Bones)
				{
					if (bone == headBone) continue;
					if (bone.Parent != null && bone.Parent.Equals(headBone.Name))
					{
						headBones.Add(bone);
					}
				}

				foreach (var bone in model.Bones.Where(x =>
					!headBones.Any(hb => hb.Name.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase))))
				{
					if (headBones.Any(x => x.Name.Equals(bone.Name, StringComparison.InvariantCultureIgnoreCase)))
					{
						headBones.Add(bone);
					}
				}
			}
			
			List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();

			var textureSize = new Vector2(model.Texturewidth, model.Textureheight);
			var newSize = new Vector2(Texture.Width, Texture.Height);

			if (textureSize.X == 0 && textureSize.Y == 0)
				textureSize = newSize;
			
			var uvScale = newSize / textureSize;
			
			foreach (var bone in model.Bones)
			{
				if (bone == null || bone.NeverRender) continue;
			//	if (bone.NeverRender) continue;
				bool partOfHead = headBones.Contains(bone);

				//bone.Pivot = new Vector3(-bone.Pivot.X, bone.Pivot.Y, bone.Pivot.Z);
				List<ModelBoneCube> c = new List<ModelBoneCube>();
				ModelBone modelBone;
				
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

						//VertexPositionNormalTexture[] vertices;
						Cube built = new Cube(size * (float)cube.Inflate, textureSize);
						built.Mirrored = bone.Mirror;
						built.BuildCube(cube.Uv * uvScale);

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
				}

				modelBone = new ModelBone(c.ToArray(), bone.Parent);
				modelBone.Rotation = bone.BindPoseRotation;
				
				modelBone.UpdateRotationMatrix = !bone.NeverRender;
					if (!modelBones.TryAdd(bone.Name, modelBone))
					{
						Log.Debug($"Failed to add bone! {model.Name}:{bone.Name}");
					}
				
			}

			VertexBuffer = GpuResourceManager.GetBuffer(this, Alex.Instance.GraphicsDevice,
				VertexPositionNormalTexture.VertexDeclaration, vertices.Count, BufferUsage.None);
			VertexBuffer.SetData(vertices.ToArray());
			
			Valid = true;
		}

		private List<VertexPositionNormalTexture> ModifyCubeIndexes(List<VertexPositionNormalTexture> vertices,
			ref (VertexPositionNormalTexture[] vertices, short[] indexes) data, Vector3 offset)
		{
			var startIndex = (short)vertices.Count;
			foreach (var vertice in data.vertices)
			{
				var vertex = vertice;
				//vertex.Position += offset;
				vertices.Add(vertex);
			}
			
			//vertices.AddRange(data.vertices);
			
			for (int i = 0; i < data.indexes.Length; i++)
			{
				data.indexes[i] += startIndex;
			}

			return vertices;
		}

		private static RasterizerState RasterizerState = new RasterizerState()
		{
			DepthBias = 0.0001f,
			CullMode = CullMode.CullClockwiseFace,
			FillMode = FillMode.Solid,
			//DepthClipEnable = true,
			//ScissorTestEnable = true
		};
		
		public virtual void Render(IRenderArgs args, PlayerLocation position, bool mock)
		{
			var originalRaster = args.GraphicsDevice.RasterizerState;
			args.GraphicsDevice.RasterizerState = RasterizerState;
			args.GraphicsDevice.SetVertexBuffer(VertexBuffer);

			if (Bones == null) return;
			foreach (var bone in Bones)
			{
				bone.Value.Render(args, position, CharacterMatrix, mock);
			}

			args.GraphicsDevice.RasterizerState = originalRaster;
		}

		public Vector3 EntityColor { get; set; } = Color.White.ToVector3();
		public Vector3 DiffuseColor { get; set; } = Color.White.ToVector3();
		private Matrix CharacterMatrix { get; set; } = Matrix.Identity;
		public float Scale { get; set; } = 1f;

		public virtual void Update(IUpdateArgs args, PlayerLocation position)
		{
			if (Bones == null) return;

			CharacterMatrix = Matrix.CreateScale(Scale / 16f) *
			                         Matrix.CreateRotationY(MathUtils.ToRadians((180f - position.Yaw))) *
			                         Matrix.CreateTranslation(position);

			foreach (var bone in Bones)
			{
				bone.Value.Update(args, CharacterMatrix, EntityColor * DiffuseColor);
			}

			foreach (var bone in Bones.Where(x => !string.IsNullOrWhiteSpace(x.Value.Parent)))
			{
				var parent = Bones.FirstOrDefault(x =>
					x.Key.Equals(bone.Value.Parent, StringComparison.InvariantCultureIgnoreCase));

				if (parent.Value != null)
				{
					bone.Value.RotationMatrix = parent.Value.RotationMatrix;
				}
			}
		}

		public bool GetBone(string name, out ModelBone bone)
		{
			if (string.IsNullOrWhiteSpace(name) || Bones == null || Bones.Count == 0)
			{
				bone = null;
				return false;
			}

			return Bones.TryGetValue(name, out bone);
		}

		/*public override string ToString()
		{
			return Model.Name;
		}
*/
		public void Dispose()
		{
			if (Bones != null && Bones.Any())
			{
				foreach (var bone in Bones.ToArray())
				{
					bone.Value.Dispose();
				}
			}
			
			Texture?.MarkForDisposal();
			VertexBuffer?.MarkForDisposal();
		}
	}
}
