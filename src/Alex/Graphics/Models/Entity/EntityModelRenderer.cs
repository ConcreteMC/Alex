using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using NLog;
using MathF = System.MathF;

namespace Alex.Graphics.Models.Entity
{
	public partial class EntityModelRenderer : Model, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityModelRenderer));

		//private EntityModel Model { get; }
		private IReadOnlyDictionary<string, ModelBone> Bones { get; }
		
		private PooledTexture2D _texture;

		public PooledTexture2D Texture
		{
			get
			{
				return _texture;
			}
			set
			{
				_texture = value;
				var bones = Bones;

				if (bones != null && bones.Count > 0)
				{
					foreach (var kv in bones)
					{
						var bone = kv.Value;
						bone.SetTexture(_texture);
					}
				}
			}
		}

		private PooledVertexBuffer VertexBuffer { get; set; }
		public bool Valid { get; private set; }
		private bool CanRender { get; set; } = true;

		public long Vertices => CanRender && VertexBuffer != null ? VertexBuffer.VertexCount : 0;
		//public float Height { get; private set; } = 0f;
		public EntityModelRenderer(EntityModel model, PooledTexture2D texture)
		{
			//	Model = model;
			if (texture == null)
			{
				Log.Warn($"No texture set for rendererer for {model.Name}!");
				return;
			}

			if (model != null)
			{
				var bones = new Dictionary<string, ModelBone>();
				Cache(texture, model, bones);

				Bones = bones;

				Valid = bones.Count > 0;
			}
		}

		private void Cache(PooledTexture2D texture, EntityModel model, Dictionary<string, ModelBone> modelBones)
		{
			List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();

			var modelTextureSize = model.Description != null ?
				new Vector2(model.Description.TextureWidth, model.Description.TextureHeight) :
				new Vector2(model.Texturewidth, model.Textureheight);
			
			var actualTextureSize = new Vector2(texture.Width, texture.Height);
				
			if (modelTextureSize.X == 0 && modelTextureSize.Y == 0)
				modelTextureSize = actualTextureSize;
			
			var uvScale = actualTextureSize / modelTextureSize;
			
			foreach (var bone in model.Bones.Where(x => string.IsNullOrWhiteSpace(x.Parent)))
			{
				//if (bone.NeverRender) continue;
				if (modelBones.ContainsKey(bone.Name)) continue;
				
				var processed = ProcessBone(texture, model, bone, vertices, uvScale, modelTextureSize, modelBones);
				
				if (!modelBones.TryAdd(bone.Name, processed))
				{
					Log.Warn($"Failed to add bone! {bone.Name}");
				}
			}

			if (vertices.Count == 0)
			{
				Log.Warn($"No vertices. {JsonConvert.SerializeObject(model,Formatting.Indented)}");
				
				Valid = true;
				CanRender = false;
				return;
			}

			VertexBuffer = GpuResourceManager.GetBuffer(this, Alex.Instance.GraphicsDevice,
				VertexPositionNormalTexture.VertexDeclaration, vertices.Count, BufferUsage.None);
			VertexBuffer.SetData(vertices.ToArray());
			
			Valid = true;
			_texture = texture;
		}

		private static Vector3 FlipX(Vector3 origin, Vector3 size)
		{
			if (origin.X >= 0)
			{
				origin.X = -(((MathF.Abs(origin.X) / size.X) + 1) * size.X);
			}
			else
			{
				origin.X = ((MathF.Abs(origin.X) / size.X) - 1) * size.X;
			}

			return origin;
		}

		private ModelBone ProcessBone(PooledTexture2D texture, EntityModel source, EntityModelBone bone, List<VertexPositionNormalTexture> vertices, Vector2 uvScale, Vector2 textureSize, Dictionary<string, ModelBone> modelBones)
		{
			ModelBone           modelBone;
				
			List<short> indices = new List<short>();

			bone.Pivot *= new Vector3(-1f, 1f, 1f);
			
			if (bone.Cubes != null)
			{
				foreach (var cube in bone.Cubes)
				{
					if (cube == null)
					{
						Log.Warn("Cube was null!");
						continue;
					}

					//cube.Origin = FlipX(cube.Origin, cube.Size);

					var size = cube.Size;

					Cube built = new Cube(cube.InflatedSize, textureSize, cube.Uv, uvScale, (float) cube.Inflate, bone.Mirror);
					//built.Mirrored = bone.Mirror;
					//built.BuildCube(cube.Uv, uvScale);
					
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Front, bone.Mirror);
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Back, bone.Mirror);
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Top, bone.Mirror);
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Bottom, bone.Mirror);
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Left, bone.Mirror);
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Right, bone.Mirror);

					indices.AddRange(built.Front.indexes.Concat(built.Back.indexes).Concat(built.Top.indexes)
					   .Concat(built.Bottom.indexes).Concat(built.Left.indexes).Concat(built.Right.indexes)
					   .ToArray());
				}
			}
			
			var bindPoseMatrix = Matrix.CreateTranslation(-bone.Pivot)
			                     * Matrix.CreateRotationY(MathUtils.ToRadians(bone.BindPoseRotation.Y))
			                     * Matrix.CreateRotationX(MathUtils.ToRadians(-bone.BindPoseRotation.X))
			                     * Matrix.CreateRotationZ(MathUtils.ToRadians(bone.BindPoseRotation.Z))
			                     * Matrix.CreateTranslation(bone.Pivot);

			/*var boneMatrix = Matrix.Identity * Matrix.CreateTranslation(-bone.Pivot)
			                                 * Matrix.CreateFromAxisAngle(
				                                 Vector3.Right, MathUtils.ToRadians(bone.Rotation.X))
			                                 * Matrix.CreateFromAxisAngle(
				                                 Vector3.Backward, MathUtils.ToRadians(bone.Rotation.Z))
			                                 * Matrix.CreateFromAxisAngle(
				                                 Vector3.Up, MathUtils.ToRadians(bone.Rotation.Y))
			                                 * Matrix.CreateTranslation(bone.Pivot);*/
			var boneMatrix =
			                  Matrix.CreateTranslation(-bone.Pivot)
			                  * Matrix.CreateRotationY(MathUtils.ToRadians( bone.Rotation.Y))
			                 * Matrix.CreateRotationX(MathUtils.ToRadians(-bone.Rotation.X))
			                  * Matrix.CreateRotationZ(MathUtils.ToRadians(bone.Rotation.Z))
			                 * Matrix.CreateTranslation(bone.Pivot);

			modelBone = new ModelBone(texture, indices.ToArray(), bone,  bindPoseMatrix * boneMatrix);

			foreach (var childBone in source.Bones.Where(
				x => string.Equals(x.Parent, bone.Name, StringComparison.InvariantCultureIgnoreCase)))
			{
				var child = ProcessBone(texture, source, childBone, vertices, uvScale, textureSize, modelBones);
				child.Parent = modelBone;
				
				modelBone.AddChild(child);
				
				if (!modelBones.TryAdd(childBone.Name, child))
				{
					Log.Warn($"Failed to add bone! {childBone.Name}");
				}
			}

			return modelBone;
		}

		private List<VertexPositionNormalTexture> ModifyCubeIndexes(List<VertexPositionNormalTexture> vertices, EntityModelCube cube,
			ref (VertexPositionNormalTexture[] vertices, short[] indexes) data, bool mirror)
		{
			var origin = FlipX(cube.InflatedOrigin, cube.InflatedSize);
			
			var pivot = cube.Pivot;
			var rotation = cube.Rotation;

			Matrix cubeRotationMatrix = Matrix.CreateTranslation(origin);

			if (rotation != Vector3.Zero)
			{
				cubeRotationMatrix *= Matrix.CreateTranslation(-pivot)
				                      * Matrix.CreateRotationY(MathUtils.ToRadians(-rotation.Y))
				                      * Matrix.CreateRotationX(MathUtils.ToRadians(-rotation.X))
				                      * Matrix.CreateRotationZ(MathUtils.ToRadians(rotation.Z))
				                      * Matrix.CreateTranslation(pivot);
			}
			
			var startIndex = (short)vertices.Count;
			foreach (var vertice in data.vertices)
			{
				var vertex = vertice;

				if ((cube.Mirror.HasValue && !cube.Mirror.Value) || (!cube.Mirror.HasValue))
				{
					//vertex.Position = RotateAboutOrigin(vertex.Position, sizeDivide, MathUtils.ToRadians(180f));
				}

				vertex.Position = Vector3.Transform(vertex.Position, cubeRotationMatrix);
				//vertex.Position = Vector3.Transform(vertex.Position, Matrix.CreateTranslation(origin));
				vertices.Add(vertex);
			}

			for (int i = 0; i < data.indexes.Length; i++)
			{
				data.indexes[i] += startIndex;
			}

			return vertices;
		}
		
		public Vector3 RotateAboutOrigin(Vector3 point, Vector3 origin, float rotation)
		{
			return Vector3.Transform(point - origin, Matrix.CreateRotationY(rotation)) + origin;
		} 

		private static RasterizerState RasterizerState = new RasterizerState()
		{
			DepthBias = 0f,
			CullMode = CullMode.CullClockwiseFace,
			FillMode = FillMode.Solid
		};
		
		public virtual void Render(IRenderArgs args, bool mock)
		{
			if (!CanRender)
			{
				Log.Warn($"Cannot render model...");
				return;
			}

			var originalRaster = args.GraphicsDevice.RasterizerState;
			var blendState = args.GraphicsDevice.BlendState;

			try
			{
				args.GraphicsDevice.BlendState = BlendState.Opaque;
				args.GraphicsDevice.RasterizerState = RasterizerState;
				
				args.GraphicsDevice.SetVertexBuffer(VertexBuffer);

				if (Bones == null)
				{
					Log.Warn($"No bones found for model...");
					return;
				}

				int rendered = 0;
				foreach (var bone in Bones.Where(x => x.Value.Parent == null))
				{
				//	if (bone.Value.Parent != null)
						bone.Value.Render(args, mock, out rendered);

						//rendered++;
				}

				if (!mock)
				{
				//	Log.Info($"Rendered {rendered} vertices");
				}
			}
			finally
			{
				args.GraphicsDevice.RasterizerState = originalRaster;
				args.GraphicsDevice.BlendState = blendState;
			}
		}

		public Vector3 EntityColor { get; set; } = Color.White.ToVector3();
		public Vector3 DiffuseColor { get; set; } = Color.White.ToVector3();

		public float Scale { get; set; } = 1f;

		public virtual void Update(IUpdateArgs args, PlayerLocation position)
		{
			if (Bones == null) return;

			foreach (var bone in Bones.Where(x => x.Value.Parent == null))
			{
				bone.Value.Update(
					args,
					Matrix.CreateRotationY(MathUtils.ToRadians(180f)) * Matrix.CreateScale(Scale / 16f) * Matrix.CreateRotationY(MathUtils.ToRadians(-(position.Yaw)))
					                                * Matrix.CreateTranslation(position), EntityColor * DiffuseColor, position);
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
		
		public void SetVisibility(string bone, bool visible)
		{
			if (GetBone(bone, out var boneValue))
			{
				boneValue.Rendered = visible;
			}
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
			
			Texture?.MarkForDisposal();
			VertexBuffer?.MarkForDisposal();
		}
	}
}
