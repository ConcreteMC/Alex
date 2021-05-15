using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.API.Graphics;
using Alex.API.Graphics.GpuResources;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Alex.Graphics.Models.Items;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
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
		
		private IReadOnlyDictionary<string, ModelBone> Bones { get; set; }

		private ManagedVertexBuffer VertexBuffer
		{
			get => _vertexBuffer;
			set
			{
				var previousValue = _vertexBuffer;

				if (value != null)
				{
					value.Use(this);
				}

				if (previousValue != null)
				{
					previousValue.Release(this);
					previousValue.ReturnResource(this);
				}
				
				_vertexBuffer = value;
			}
		}

		private ManagedIndexBuffer IndexBuffer
		{
			get => _indexBuffer;
			set
			{
				var previousValue = _indexBuffer;

				if (value != null)
				{
					value.Use(this);
				}

				if (previousValue != null)
				{
					previousValue.Release(this);
					previousValue.ReturnResource(this);
				}
				_indexBuffer = value;
			}
		}
		//public  bool               Valid        { get; private set; }

		public double VisibleBoundsWidth { get; set; } = 0;

		public double VisibleBoundsHeight { get; set; } = 0;

		public EntityModelRenderer()
		{
			//	Model = model;
			
		}


		private class SharedBuffer
		{
			public SharedBuffer(ManagedVertexBuffer vertexBuffer, ManagedIndexBuffer indexBuffer, IReadOnlyDictionary<string, ModelBone> bones)
			{
				VertexBuffer = vertexBuffer;
				IndexBuffer = indexBuffer;
				Bones = bones;
				//Vertices = vertices;
				//Indices = indices;
			}

			public ManagedVertexBuffer VertexBuffer { get; }
			public ManagedIndexBuffer IndexBuffer { get; }
			public IReadOnlyDictionary<string, ModelBone> Bones { get; }
			//public List<VertexPositionColorTexture> Vertices { get; }
			//public List<short> Indices { get; }
		}

		private static ConcurrentDictionary<string, SharedBuffer> _sharedBuffers =
			new ConcurrentDictionary<string, SharedBuffer>();
		
		
		public static bool TryGetRenderer(EntityModel model, out EntityModelRenderer renderer)
		{
			{
				var tuple = _sharedBuffers.GetOrAdd(model.Description.Identifier, s =>
				{
					List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
					List<short> indices = new List<short>();
					var bones = new Dictionary<string, ModelBone>(StringComparer.OrdinalIgnoreCase);

					if (BuildModel(model, bones, vertices, indices))
					{

						var indexBuffer = GpuResourceManager.GetIndexBuffer(
							_sharedBuffers, Alex.Instance.GraphicsDevice, IndexElementSize.SixteenBits, indices.Count,
							BufferUsage.WriteOnly);

						indexBuffer.SetData(indices.ToArray());

						indexBuffer.ResourceDisposed += (sender, resource) =>
						{
							_sharedBuffers.TryRemove(model.Description.Identifier, out _);
						};

						var vertexBuffer = GpuResourceManager.GetBuffer(
							_sharedBuffers, Alex.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration,
							vertices.Count, BufferUsage.WriteOnly);

						vertexBuffer.SetData(vertices.ToArray());

						vertexBuffer.ResourceDisposed += (sender, resource) =>
						{
							_sharedBuffers.TryRemove(model.Description.Identifier, out _);
						};

						return new SharedBuffer(vertexBuffer, indexBuffer, bones);
					}

					return null;
				});

				//tuple.Item1.Use(renderer);
				//tuple.Item2.Use(renderer);
				
				renderer = new EntityModelRenderer();
				renderer.VisibleBoundsWidth = model.Description.VisibleBoundsWidth;
				renderer.VisibleBoundsHeight = model.Description.VisibleBoundsHeight;

				Dictionary<string, ModelBone> clonedBones = new Dictionary<string, ModelBone>(StringComparer.OrdinalIgnoreCase);

				foreach (var bone in tuple.Bones.Where(x => x.Value.Parent == null)) //We only wanna clone the root bones
				{
					if (bone.Value.Clone() is ModelBone boneClone)
					{
						clonedBones.Add(bone.Key, boneClone);

						foreach (var child in GetAllChildren(boneClone))
						{
							if (!clonedBones.ContainsKey(child.Name))
								clonedBones.Add(child.Name, child);
						}
					}
				}
				renderer.Bones = clonedBones;
				
				renderer.IndexBuffer = tuple.IndexBuffer;
				renderer.VertexBuffer = tuple.VertexBuffer;
				
				//renderer.IndexBuffer = GpuResourceManager.GetIndexBuffer(
				//	renderer, Alex.Instance.GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
				//renderer.IndexBuffer.SetData(indices.ToArray());
			
			//	renderer.VertexBuffer = GpuResourceManager.GetBuffer(renderer, Alex.Instance.GraphicsDevice,
			//		VertexPositionColorTexture.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
			//	renderer.VertexBuffer.SetData(vertices.ToArray());

				return true;
			}

			renderer = null;
			return false;
		}

		private static IEnumerable<ModelBone> GetAllChildren(ModelBone root)
		{
			foreach (var child in root.Children)
			{
				if (child is ModelBone modelBone)
				{
					yield return modelBone;

					foreach (var subChild in GetAllChildren(modelBone))
					{
						yield return subChild;
					}
				}
			}
		}

		private static bool BuildModel(EntityModel model, Dictionary<string, ModelBone> modelBones, List<VertexPositionColorTexture> vertices, List<short> indices)
		{
			//var actualTextureSize = new Vector2(texture.Width, texture.Height);

			int counter = 0;
			foreach (var bone in model.Bones.Where(x => string.IsNullOrWhiteSpace(x.Parent)))
			{
				//if (bone.NeverRender) continue;
				if (string.IsNullOrWhiteSpace(bone.Name))
					bone.Name = $"bone{counter++}";
				
				if (modelBones.ContainsKey(bone.Name)) continue;
				
				var processed = ProcessBone(model, bone, ref vertices, ref indices,  modelBones);
				
				if (!modelBones.TryAdd(bone.Name, processed))
				{
					Log.Warn($"Failed to add bone! {bone.Name}");
				}
			}

			if (vertices.Count == 0 || indices.Count == 0)
			{
				Log.Warn($"Oh no. Vertices: {vertices.Count} Indices: {indices.Count}");
				return false;
			}

			return true;
			//Texture = texture;
		}

		private static ModelBone ProcessBone(
			EntityModel source,
			EntityModelBone bone,
			ref List<VertexPositionColorTexture> vertices,
			ref List<short> indices,
			Dictionary<string, ModelBone> modelBones)
		{
			ModelBone modelBone = new ModelBone();
			int           startIndex   = indices.Count;
			int           elementCount = 0;
			if (bone.Cubes != null)
			{
				foreach (var cube in bone.Cubes)
				{
					if (cube == null)
					{
						Log.Warn("Cube was null!");

						continue;
					}
					
					var inflation = (float) (cube.Inflate ?? bone.Inflate);
					var mirror    = cube.Mirror ?? bone.Mirror;

					var      origin = cube.InflatedOrigin(inflation);
					
					Matrix matrix = Matrix.CreateTranslation(origin);
					if (cube.Rotation.HasValue)
					{
						var rotation = cube.Rotation.Value;
						
						Vector3 pivot = origin + (cube.InflatedSize(inflation) / 2f);

						if (cube.Pivot.HasValue)
						{
							pivot = cube.InflatedPivot(inflation);// cube.Pivot.Value;
						}
						
						matrix =
							    Matrix.CreateTranslation(origin)
								* Matrix.CreateTranslation((-pivot)) 
						         * MatrixHelper.CreateRotationDegrees(rotation)
						         * Matrix.CreateTranslation(pivot);
					}
					
					Cube built = new Cube(cube, mirror, inflation);
					ModifyCubeIndexes(ref vertices, ref indices, built.Front, matrix);
					ModifyCubeIndexes(ref vertices, ref indices, built.Back, matrix);
					ModifyCubeIndexes(ref vertices, ref indices, built.Top, matrix);
					ModifyCubeIndexes(ref vertices, ref indices, built.Bottom, matrix);
					ModifyCubeIndexes(ref vertices, ref indices, built.Left, matrix);
					ModifyCubeIndexes(ref vertices, ref indices, built.Right, matrix);
				}
				
				elementCount = indices.Count - startIndex;
				modelBone.AddMesh(new ModelMesh(startIndex, elementCount / 3));
			}

			//startIndex, elementCount / 3

			modelBone.Pivot = bone.Pivot;
			modelBone.Name = bone.Name;
			modelBone.Rendered = !bone.NeverRender;

			if (bone.Rotation.HasValue)
			{
				var r = bone.Rotation.Value;
				modelBone.BindingRotation = new Vector3(r.X, r.Y, r.Z);
			}
			
			if (bone.BindPoseRotation.HasValue)
			{
				var r = bone.BindPoseRotation.Value;
				modelBone.BindingRotation += new Vector3(r.X, r.Y, r.Z);
			}

			foreach (var childBone in source.Bones.Where(
				x => x.Parent != null && string.Equals(x.Parent, bone.Name, StringComparison.OrdinalIgnoreCase)))
			{
				if (childBone.Parent != null && childBone.Parent.Equals(childBone.Name))
					continue;

				if (string.IsNullOrWhiteSpace(childBone.Name))
					childBone.Name = Guid.NewGuid().ToString();
				
				var child = ProcessBone(source, childBone, ref vertices, ref indices, modelBones);
				//child.Parent = modelBone;

				modelBone.AddChild(child);
				
				if (!modelBones.TryAdd(childBone.Name, child))
				{
					Log.Warn($"Failed to add bone! {childBone.Name}");
					break;
				}
			}

			return modelBone;
		}

		private static void ModifyCubeIndexes(ref List<VertexPositionColorTexture> vertices, ref List<short> indices,
			(VertexPositionColorTexture[] vertices, short[] indexes) data, Matrix transformation)
		{
			var startIndex = vertices.Count;
			
			for (int i = 0; i < data.vertices.Length; i++)
			{
				var vertex = data.vertices[i];
				var position =  Vector3.Transform(vertex.Position, transformation);
				vertices.Add(new VertexPositionColorTexture(position, vertex.Color, vertex.TextureCoordinate));
			}
			
			for (int i = 0; i < data.indexes.Length; i++)
			{
				var listIndex = startIndex + data.indexes[i];
				indices.Add((short) listIndex);
			}
		}
		
		private static readonly RasterizerState _rasterizerState = new RasterizerState()
		{
			DepthBias = 0f,
			CullMode = CullMode.None,
			FillMode = FillMode.Solid,
			ScissorTestEnable = false
		};
		
		private static readonly RasterizerState _rasterizerStateCulled = new RasterizerState()
		{
			DepthBias = 0f,
			CullMode = CullMode.CullClockwiseFace,
			FillMode = FillMode.Solid,
			ScissorTestEnable = false
		};
		
		/// <summary>
		///		Renders the entity model
		/// </summary>
		/// <param name="args"></param>
		/// <returns>The amount of GraphicsDevice.Draw calls made</returns>
		public virtual int Render(IRenderArgs args, bool useCulling, Microsoft.Xna.Framework.Graphics.Effect effect, Matrix worldMatrix)
		{
			if (Bones == null || VertexBuffer == null || IndexBuffer == null)
			{
				//Log.Warn($"No bones found for model...");
				return 0;
			}

			var originalRaster = args.GraphicsDevice.RasterizerState;
			var blendState = args.GraphicsDevice.BlendState;

			int counter = 0;
			try
			{
				args.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				args.GraphicsDevice.RasterizerState = useCulling ? _rasterizerStateCulled : _rasterizerState;

				args.GraphicsDevice.Indices = IndexBuffer;
				args.GraphicsDevice.SetVertexBuffer(VertexBuffer);

				var newArgs = new AttachedRenderArgs()
				{
					Buffer = VertexBuffer,
					Camera = args.Camera,
					GameTime = args.GameTime,
					GraphicsDevice = args.GraphicsDevice,
					SpriteBatch = args.SpriteBatch
				};
				
				var matrix =  worldMatrix;
				;
				foreach (var bone in Bones.Where(x => x.Value.Parent == null))
				{
					counter += bone.Value.Render(newArgs, effect,  matrix);
					//RenderBone(args, bone.Value);
				}
			}
			finally
			{
				args.GraphicsDevice.RasterizerState = originalRaster;
				args.GraphicsDevice.BlendState = blendState;
			}

			return counter;
		}

		public Vector3 EntityColor { get; set; } = Color.White.ToVector3();
		public Vector3 DiffuseColor { get; set; } = Color.White.ToVector3();

		public float Scale { get; set; } = 1f;
		
		public virtual void Update(IUpdateArgs args)
		{
			if (Bones == null) return;

			foreach (var bone in Bones.Where(x => x.Value.Parent == null))
			{
				bone.Value.Update(args, Vector3.One * Scale);
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

		//private int _instances = 0;
		private ManagedIndexBuffer _indexBuffer;
		private ManagedVertexBuffer _vertexBuffer;

		public void Dispose()
		{
			//if (Interlocked.Decrement(ref _instances) == 0)
			{
				//Effect?.Dispose();

				//Effect = null;
				VertexBuffer = null;
				IndexBuffer = null;
				//Texture = null;
			}

			return;
		}

		public void ApplyPending()
		{
			foreach (var b in Bones)
			{
				b.Value?.ApplyMovement();
			}
		}
	}
}
