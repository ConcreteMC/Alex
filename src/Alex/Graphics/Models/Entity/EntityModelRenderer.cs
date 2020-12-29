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
				Log.Warn($"No texture set for rendererer for {model}!");
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
			List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();

			//var modelTextureSize = model.Description != null ?
			//	new Vector2(model.Description.TextureWidth, model.Description.TextureHeight) :
			//	new Vector2(model.Texturewidth, model.Textureheight);
			
			//var modelTextureSize = new Vector2(model.Description.TextureWidth, model.Description.TextureHeight);
			
			var actualTextureSize = new Vector2(texture.Width, texture.Height);

			foreach (var bone in model.Bones.Where(x => string.IsNullOrWhiteSpace(x.Parent)))
			{
				//if (bone.NeverRender) continue;
				if (modelBones.ContainsKey(bone.Name)) continue;
				
				var processed = ProcessBone(texture, model, bone, vertices, actualTextureSize , modelBones);
				
				if (!modelBones.TryAdd(bone.Name, processed))
				{
					Log.Warn($"Failed to add bone! {bone.Name}");
				}
			}

			if (vertices.Count == 0)
			{
				//Log.Warn($"No vertices.");
				
				Valid = true;
				CanRender = false;
				return;
			}
			
			VertexBuffer = GpuResourceManager.GetBuffer(this, Alex.Instance.GraphicsDevice,
				VertexPositionColorTexture.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
			VertexBuffer.SetData(vertices.ToArray());
			
			Valid = true;
			_texture = texture;
		}

		private static Vector3 FlipX(Vector3 origin, Vector3 size)
		{
			//return origin;
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
		
		private static Vector3 FlipZ(Vector3 origin, Vector3 size)
		{
			//return origin;
			if (origin.Z >= 0)
			{
				origin.Z = -(((MathF.Abs(origin.Z) / size.Z) + 1) * size.Z);
			}
			else
			{
				origin.Z = ((MathF.Abs(origin.Z) / size.Z) - 1) * size.Z;
			}

			return origin;
		}

		private ModelBone ProcessBone(PooledTexture2D texture,
			EntityModel source,
			EntityModelBone bone,
			List<VertexPositionColorTexture> vertices,
			Vector2 textureSize,
			Dictionary<string, ModelBone> modelBones)
		{
			ModelBone modelBone;

			List<short> indices = new List<short>();

			//bone.Pivot *= new Vector3(-1f, 1f, 1f);

			if (bone.Cubes != null)
			{
				foreach (var cube in bone.Cubes)
				{
					if (cube == null)
					{
						Log.Warn("Cube was null!");

						continue;
					}

					//if (cube.Uv.IsOutOfBound(textureSize))
					{
					//	continue;
					}

					var  inflation = (float) (cube.Inflate ?? bone.Inflate);
					var  mirror    = cube.Mirror ?? bone.Mirror;
					Cube built     = new Cube(source, cube, textureSize, mirror, inflation);

					vertices = ModifyCubeIndexes(vertices, cube, ref built.Front, bone, inflation);
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Back, bone, inflation);
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Top, bone, inflation);
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Bottom, bone, inflation);
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Left, bone, inflation);
					vertices = ModifyCubeIndexes(vertices, cube, ref built.Right, bone, inflation);

					indices.AddRange(
						built.Front.indexes.Concat(built.Back.indexes).Concat(built.Top.indexes)
						   .Concat(built.Bottom.indexes).Concat(built.Left.indexes).Concat(built.Right.indexes)
						   .ToArray());
				}
			}

			var boneMatrix = Matrix.Identity;

			if (bone.BindPoseRotation.HasValue)
			{
				var rotation = bone.BindPoseRotation.Value;

				boneMatrix = Matrix.CreateTranslation(-bone.Pivot)
				             * Matrix.CreateRotationX(MathUtils.ToRadians(-rotation.X))
				             * Matrix.CreateRotationY(MathUtils.ToRadians(rotation.Y))
				             * Matrix.CreateRotationZ(MathUtils.ToRadians(rotation.Z))
				             * Matrix.CreateTranslation(bone.Pivot);
			}

			modelBone = new ModelBone(texture, indices.ToArray(), bone, boneMatrix);

			if (bone.Rotation.HasValue)
			{
				var r = bone.Rotation.Value;
				modelBone.Rotation = new Vector3(-r.X, r.Y, r.Z);
			}

			modelBone.Setup(Alex.Instance.GraphicsDevice);

			foreach (var childBone in source.Bones.Where(
				x => string.Equals(x.Parent, bone.Name, StringComparison.OrdinalIgnoreCase)))
			{
				var child = ProcessBone(texture, source, childBone, vertices, textureSize, modelBones);
				child.Parent = modelBone;

				modelBone.AddChild(child);

				if (!modelBones.TryAdd(childBone.Name, child))
				{
					Log.Warn($"Failed to add bone! {childBone.Name}");
				}
			}

			return modelBone;
		}

		private List<VertexPositionColorTexture> ModifyCubeIndexes(List<VertexPositionColorTexture> vertices, EntityModelCube cube,
			ref (VertexPositionColorTexture[] vertices, short[] indexes) data, EntityModelBone bone, float inflate)
		{
		//	var pivot = cube.Pivot.HasValue ? cube.Pivot.Value : (cube.InflatedSize(inflate) * new Vector3(0.5f, 0.5f,0.5f));

			var startIndex = (short)vertices.Count;
			vertices.AddRange(data.vertices);
		//	var or         = cube.InflatedOrigin(inflate);//, cube.InflatedSize(inflate));
			/*foreach (var vertice in data.vertices)
			{
				var vertex = vertice;
				vertices.Add(vertex);
			}*/

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
				//Log.Warn($"Cannot render model...");
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

			var matrix = Matrix.CreateRotationY(MathUtils.ToRadians(180f)) * Matrix.CreateScale(Scale / 16f)
			                                                               * Matrix.CreateRotationY(
				                                                               MathUtils.ToRadians(-(position.Yaw)))
			                                                               * Matrix.CreateTranslation(position);
			
			foreach (var bone in Bones.Where(x => x.Value.Parent == null))
			{
				bone.Value.Update(
					args, matrix, EntityColor * DiffuseColor, position);
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
