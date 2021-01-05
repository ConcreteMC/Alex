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

				if (Effect != null)
				{
					Effect.Texture = value;
				}
			}
		}

		private AlphaTestEffect    Effect       { get; set; }
		private PooledVertexBuffer VertexBuffer { get; set; }
		public  bool               Valid        { get; private set; }
		private bool               CanRender    { get; set; } = true;

		public long        Vertices => CanRender && VertexBuffer != null ? VertexBuffer.VertexCount : 0;
		public EntityModel Model    { get; }
		public EntityModelRenderer(EntityModel model, PooledTexture2D texture)
		{
			Model = model;
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
			}
		}

		private void Cache(PooledTexture2D texture, EntityModel model, Dictionary<string, ModelBone> modelBones)
		{
			List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();

			var actualTextureSize = new Vector2(texture.Width, texture.Height);

			int counter = 0;
			foreach (var bone in model.Bones.Where(x => string.IsNullOrWhiteSpace(x.Parent)))
			{
				//if (bone.NeverRender) continue;
				if (string.IsNullOrWhiteSpace(bone.Name))
					bone.Name = $"bone{counter++}";
				
				if (modelBones.ContainsKey(bone.Name)) continue;
				
				var processed = ProcessBone(model, bone, ref vertices, actualTextureSize , modelBones);
				
				if (!modelBones.TryAdd(bone.Name, processed))
				{
					Log.Warn($"Failed to add bone! {bone.Name}");
				}
			}

			if (vertices.Count == 0)
			{
				Valid = true;
				CanRender = false;
				return;
			}
			
			VertexBuffer = GpuResourceManager.GetBuffer(this, Alex.Instance.GraphicsDevice,
				VertexPositionColorTexture.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
			VertexBuffer.SetData(vertices.ToArray());

			Effect = new AlphaTestEffect(texture.GraphicsDevice);
			Effect.Texture = texture;
			Effect.VertexColorEnabled = true;
			
			Valid = true;
			_texture = texture;
		}

		private ModelBone ProcessBone(
			EntityModel source,
			EntityModelBone bone,
			ref List<VertexPositionColorTexture> vertices,
			Vector2 textureSize,
			Dictionary<string, ModelBone> modelBones)
		{
			ModelBone modelBone;

				//List<short> indices = new List<short>();

			//bone.Pivot *= new Vector3(-1f, 1f, 1f);

			int startIndex   = vertices.Count;
			int elementCount = 0;
			if (bone.Cubes != null)
			{
				foreach (var cube in bone.Cubes)
				{
					if (cube == null)
					{
						Log.Warn("Cube was null!");

						continue;
					}

					var  inflation = (float) (cube.Inflate ?? bone.Inflate);
					var  mirror    = cube.Mirror ?? bone.Mirror;
					
					Cube built     = new Cube(source, cube, textureSize, mirror, inflation);
					ModifyCubeIndexes(ref vertices, built.Front);
					ModifyCubeIndexes(ref vertices, built.Back);
					ModifyCubeIndexes(ref vertices, built.Top);
					ModifyCubeIndexes(ref vertices, built.Bottom);
					ModifyCubeIndexes(ref vertices, built.Left);
					ModifyCubeIndexes(ref vertices, built.Right);
				}
			}

			elementCount = vertices.Count - startIndex;

			var pivot      = bone.Pivot ?? Vector3.Zero;
			var boneMatrix = Matrix.Identity;

			if (bone.BindPoseRotation.HasValue)
			{
				var rotation = bone.BindPoseRotation.Value;

				boneMatrix = Matrix.CreateTranslation(-pivot)
				             * Matrix.CreateRotationX(MathUtils.ToRadians(-rotation.X))
				             * Matrix.CreateRotationY(MathUtils.ToRadians(rotation.Y))
				             * Matrix.CreateRotationZ(MathUtils.ToRadians(rotation.Z))
				             * Matrix.CreateTranslation(pivot);
			}

			modelBone = new ModelBone(bone, boneMatrix, startIndex, elementCount);

			if (bone.Rotation.HasValue)
			{
				var r = bone.Rotation.Value;
				modelBone.Rotation = new Vector3(r.X, r.Y, r.Z);
			}

			foreach (var childBone in source.Bones.Where(
				x => x.Parent != null && string.Equals(x.Parent, bone.Name, StringComparison.OrdinalIgnoreCase)))
			{
				if (childBone.Parent != null && childBone.Parent.Equals(childBone.Name))
					continue;

				if (string.IsNullOrWhiteSpace(childBone.Name))
					childBone.Name = Guid.NewGuid().ToString();
				
				var child = ProcessBone(source, childBone, ref vertices, textureSize, modelBones);
				child.Parent = modelBone;

				modelBone.AddChild(child);
				
				if (!modelBones.TryAdd(childBone.Name, child))
				{
					Log.Warn($"Failed to add bone! {childBone.Name}");
					break;
				}
			}

			return modelBone;
		}

		private void ModifyCubeIndexes(ref List<VertexPositionColorTexture> vertices,
			(VertexPositionColorTexture[] vertices, short[] indexes) data)
		{
			for (int i = 0; i < data.indexes.Length; i++)
			{
				vertices.Add(data.vertices[data.indexes[i]]);
			}
		}
		
		private static RasterizerState RasterizerState = new RasterizerState()
		{
			DepthBias = 0f,
			CullMode = CullMode.CullCounterClockwiseFace,
			FillMode = FillMode.Solid
		};
		
		public virtual void Render(IRenderArgs args)
		{
			if (!CanRender)
			{
				//Log.Warn($"Cannot render model...");
				return;
			}
			
			if (Bones == null)
			{
				Log.Warn($"No bones found for model...");
				return;
			}

			var originalRaster = args.GraphicsDevice.RasterizerState;
			var blendState = args.GraphicsDevice.BlendState;

			try
			{
				args.GraphicsDevice.BlendState = BlendState.Opaque;
				args.GraphicsDevice.RasterizerState = RasterizerState;

				args.GraphicsDevice.SetVertexBuffer(VertexBuffer);

				foreach (var bone in Bones.Where(x => x.Value.Parent == null))
				{
					RenderBone(args, bone.Value);
				}
			}
			finally
			{
				args.GraphicsDevice.RasterizerState = originalRaster;
				args.GraphicsDevice.BlendState = blendState;
			}
		}

		private void RenderBone(IRenderArgs args, ModelBone bone)
		{
			var count = bone.ElementCount;

			if (!bone.Definition.NeverRender && bone.Rendered && count > 0)
			{
				Effect.World = bone.WorldMatrix;

				foreach (var pass in Effect.CurrentTechnique.Passes)
				{
					pass?.Apply();

					args.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, bone.StartIndex, count / 3);
				}
			}
			
			foreach (var child in bone.Children)
			{
				RenderBone(args, child);
			}
		}
		
		public Vector3 EntityColor { get; set; } = Color.White.ToVector3();
		public Vector3 DiffuseColor { get; set; } = Color.White.ToVector3();

		public float Scale { get; set; } = 1f;

		public virtual void Update(IUpdateArgs args, PlayerLocation position)
		{
			if (Bones == null) return;

			Effect.View = args.Camera.ViewMatrix;
			Effect.Projection = args.Camera.ProjectionMatrix;
			Effect.DiffuseColor = EntityColor * DiffuseColor;
			
			var matrix =Matrix.CreateScale(Scale / 16f) * Matrix.CreateRotationY(MathUtils.ToRadians(180f))
			                                                               * Matrix.CreateRotationY(
				                                                               MathUtils.ToRadians(-(position.Yaw)))
			                                                               * Matrix.CreateTranslation(position);
			
			foreach (var bone in Bones.Where(x => x.Value.Parent == null))
			{
				bone.Value.Update(
					args, matrix, position);
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
			Effect?.Dispose();

			Effect = null;
			VertexBuffer = null;
			Texture = null;
		}
	}
}
