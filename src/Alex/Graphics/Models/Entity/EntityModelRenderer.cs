using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Common.Graphics;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Utils;
using Alex.Graphics.Effect;
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
	public partial class EntityModelRenderer : Models.Model, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityModelRenderer));
		private Vector3 _entityColor = Color.White.ToVector3();
		private Vector3 _diffuseColor = Color.White.ToVector3();
		private Model Model { get; set; }
		
		public double VisibleBoundsWidth { get; set; } = 0;
		public double VisibleBoundsHeight { get; set; } = 0;

		public Vector3 EntityColor
		{
			get => _entityColor;
			set
			{
				_entityColor = value;
				var effect = Effect;

				if (effect != null)
					effect.DiffuseColor = _diffuseColor * _entityColor;
			}
		}

		public Vector3 DiffuseColor
		{
			get => _diffuseColor;
			set
			{
				_diffuseColor = value;
				var effect = Effect;

				if (effect != null)
					effect.DiffuseColor = _diffuseColor * _entityColor;
			}
		}

		public EntityEffect Effect { get; private set; }
		public string ModelName { get; private set; }
		public EntityModelRenderer()
		{
			
			//	Model = model;
			//base.Scale = 1f;
		}

		public static bool TryGetRenderer(EntityModel model, out EntityModelRenderer renderer)
		{
			if (!BuildModel(model, out var instance))
			{
				renderer = null;
				return false;
			}
			//var sharedBuffer = BuildSharedBuffer(model);

			renderer = new EntityModelRenderer();
			renderer.ModelName = model.Description.Identifier;
			renderer.Effect = new EntityEffect
			{
				VertexColorEnabled = true,
				DiffuseColor = renderer._diffuseColor * renderer._entityColor
			};

			renderer.VisibleBoundsWidth = model.Description.VisibleBoundsWidth;
			renderer.VisibleBoundsHeight = model.Description.VisibleBoundsHeight;

			foreach (var mesh in instance.Meshes)
			{
				foreach (var part in mesh.MeshParts)
				{
					part.Effect = renderer.Effect;
				}
			}
			
			renderer.Model = instance;
			
			return true;
		}

		private static IEnumerable<ModelBone> GetChildren(ModelBone parent)
		{
			foreach (var child in parent.Children)
			{
				yield return child;

				foreach (var subChild in GetChildren(child))
				{
					yield return subChild;
				}
			}
		}

		private static bool BuildModel(EntityModel model, out Model instance)
		{
			instance = null;

			List<ModelBone> modelBoneInstances = new List<ModelBone>();
			List<ModelMesh> modelMeshInstances = new List<ModelMesh>();

			List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
			List<short> indices = new List<short>();

			List<ModelBone> rootModelBones = new List<ModelBone>();

			int counter = 0;

			foreach (var bone in model.Bones.Where(x => string.IsNullOrWhiteSpace(x.Parent)))
			{
				if (string.IsNullOrWhiteSpace(bone.Name))
					bone.Name = $"bone{counter++}";

				var processed = ProcessBone(model, bone, ref vertices, ref indices, ref modelMeshInstances);
				rootModelBones.Add(processed);
			}

			foreach (var bone in rootModelBones)
			{
				modelBoneInstances.Add(bone);

				foreach (var child in GetChildren(bone))
				{
					modelBoneInstances.Add(child);
				}
			}

			ModelBone root = new ModelBone { Name = "AlexModelRoot" };

			foreach (var child in rootModelBones)
			{
				root.AddChild(child);
			}
			
			modelBoneInstances.Add(root);

			var vertexBuffer = new VertexBuffer(
				Alex.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, vertices.Count,
				BufferUsage.WriteOnly);

			vertexBuffer.SetData(vertices.ToArray());

			var indexBuffer = new IndexBuffer(
				Alex.Instance.GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);

			indexBuffer.SetData(indices.ToArray());

			foreach (var meshInstance in modelMeshInstances)
			{
				foreach (var part in meshInstance.MeshParts)
				{
					part.VertexBuffer = vertexBuffer;
					part.IndexBuffer = indexBuffer;
				}
			}

			instance = new Model(modelBoneInstances, modelMeshInstances);

			instance.Root = root;
			instance.BuildHierarchy();

			return true;
			//Texture = texture;
		}
		

		private static ModelBone ProcessBone(
			EntityModel source,
			EntityModelBone bone,
			ref List<VertexPositionColorTexture> vertices,
			ref List<short> indices,
			ref List<ModelMesh> modelMeshInstances)
		{
			ModelBone modelBone = new ModelBone()
			{
				Name = bone.Name,
				Rendered = !bone.NeverRender
			};
			
			if (bone.Cubes != null)
			{
				List<ModelMeshPart> meshParts = new List<ModelMeshPart>();
				foreach (var cube in bone.Cubes)
				{
					int vertexStart = vertices.Count;
					int           cubeStartIndex   = indices.Count;
					
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

					ModelMeshPart part = new ModelMeshPart
					{
						StartIndex = cubeStartIndex,
						PrimitiveCount = (indices.Count - cubeStartIndex) / 3,
						NumVertices = vertices.Count - vertexStart
					};

					meshParts.Add(part);
				}

				var meshInstance = new ModelMesh(Alex.Instance.GraphicsDevice, meshParts)
				{
					Name = "Cubes"
				};

				modelMeshInstances.Add(meshInstance);
				modelBone.AddMesh(meshInstance);
			}

			modelBone.Pivot = bone.Pivot.HasValue ?
				new Vector3(bone.Pivot.Value.X, bone.Pivot.Value.Y, bone.Pivot.Value.Z) : null;
			
			modelBone.Rendered = !bone.NeverRender;
			if (bone.Rotation.HasValue)
			{
				var r = bone.Rotation.Value;
				modelBone.BaseRotation = new Vector3(r.X, r.Y, r.Z);
			}
			
			if (bone.BindPoseRotation.HasValue)
			{
				var r = bone.BindPoseRotation.Value;
				modelBone.BaseRotation += new Vector3(r.X, r.Y, r.Z);
				Log.Warn($"Got binding rotation for model: {source.Description.Identifier}");
			}
			
			foreach (var childBone in source.Bones.Where(
				x => x.Parent != null && string.Equals(x.Parent, bone.Name, StringComparison.OrdinalIgnoreCase)))
			{
				if (childBone.Parent != null && childBone.Parent.Equals(childBone.Name, StringComparison.OrdinalIgnoreCase))
					continue;

				if (string.IsNullOrWhiteSpace(childBone.Name))
					childBone.Name = Guid.NewGuid().ToString();
				
				var child = ProcessBone(source, childBone, ref vertices, ref indices, ref modelMeshInstances);
				modelBone.AddChild(child);
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

		///  <summary>
		/// 		Renders the entity model
		///  </summary>
		///  <param name="args"></param>
		///  <param name="worldMatrix">The world matrix</param>
		///  <returns>The amount of GraphicsDevice.Draw calls made</returns>
		public virtual int Render(IRenderArgs args, Matrix worldMatrix)
		{
			var modelInstance = Model;

			if (modelInstance == null)
				return 0;
			
			return modelInstance.Draw(worldMatrix, args.Camera.ViewMatrix, args.Camera.ProjectionMatrix);
		}

		public virtual void Update(IUpdateArgs args)
		{
			var model = Model;
			if (model == null) return;

			model.Update(args);
		}

		public bool GetBone(string name, out ModelBone bone)
		{
			if (Model.Bones.TryGetValue(name, out bone))
			{
				return true;
			}

			return false;
		}
		
		public void SetVisibility(string bone, bool visible)
		{
			if (GetBone(bone, out var boneValue))
			{
				boneValue.Rendered = visible;
			}
		}

		//private int _instances = 0;

		public void Dispose()
		{
			Effect?.Dispose();
			Effect = null;
		}

		public void ApplyPending()
		{
			var modelInstance = Model;

			if (modelInstance == null)
				return;
			
			foreach (var b in modelInstance.Bones)
			{
				b?.ApplyMovement();
			}
		}
	}
}
