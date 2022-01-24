using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.Common.Utils;
using Alex.Graphics.Effect;
using Alex.Graphics.Models;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Model = Alex.Graphics.Models.Model;
using ModelBone = Alex.Graphics.Models.ModelBone;
using ModelMesh = Alex.Graphics.Models.ModelMesh;
using ModelMeshPart = Alex.Graphics.Models.ModelMeshPart;

namespace Alex.Utils
{
	public static class EntityModelExtensions
	{
		private static readonly StringComparison DefaultComparison = StringComparison.OrdinalIgnoreCase;

		public static bool TryGetRenderer(this EntityModel model, out ModelRenderer renderer)
		{
			IModel instance;

			bool isNewModel = false;

			if (!_models.TryGetValue(model.Description.Identifier, out instance))
			{
				if (!model.BuildModel(out instance))
				{
					renderer = null;

					return false;
				}

				isNewModel = true;

				if (_models.TryAdd(model.Description.Identifier, instance))
				{
					instance.Disposed += (sender, args) => { _models.TryRemove(model.Description.Identifier, out _); };
				}
			}

			var textureSize = new Vector2(model.Description.TextureWidth, model.Description.TextureHeight);

			if (isNewModel)
			{
				var newEffect = new EntityEffect()
				{
					VertexColorEnabled = true, TextureScale = Vector2.One / textureSize, FogEnabled = false
				};

				foreach (var mesh in instance.Meshes)
				{
					foreach (var part in mesh.MeshParts)
					{
						part.Effect = newEffect;
					}
				}
			}

			if (instance is Model m)
			{
				instance = m.Instanced();
			}

			var r = new ModelRenderer(
				instance,
				new EntityEffect()
				{
					VertexColorEnabled = true, TextureScale = Vector2.One / textureSize, FogEnabled = false
				})
			{
				VisibleBoundsWidth = model.Description.VisibleBoundsWidth,
				VisibleBoundsHeight = model.Description.VisibleBoundsHeight,
				TextureSize = textureSize
			};

			renderer = r;

			return true;
		}

		private static ConcurrentDictionary<string, IModel> _models =
			new ConcurrentDictionary<string, IModel>(StringComparer.InvariantCultureIgnoreCase);

		private static bool BuildModel(this EntityModel model, out IModel instance)
		{
			instance = null;

			List<ModelBone> modelBoneInstances = new List<ModelBone>();
			List<ModelMesh> modelMeshInstances = new List<ModelMesh>();

			List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
			List<short> indices = new List<short>();

			ModelBone root;

			var rootDefinition = model.Bones.FirstOrDefault(
				x => string.IsNullOrWhiteSpace(x.Parent) && string.Equals(
					x?.Name, "root", StringComparison.InvariantCultureIgnoreCase));

			if (rootDefinition != null)
			{
				root = ProcessBone(model, rootDefinition, ref vertices, ref indices);
			}
			else
			{
				root = new ModelBone { Name = "AlexModelRoot" };
			}

			modelBoneInstances.Add(root);

			int counter = 0;

			foreach (var bone in model.Bones.Where(x => string.IsNullOrWhiteSpace(x.Parent) && x != rootDefinition))
			{
				if (string.IsNullOrWhiteSpace(bone.Name))
					bone.Name = $"bone{counter++}";

				var processed = ProcessBone(model, bone, ref vertices, ref indices);
				root.AddChild(processed);
			}

			foreach (var bone in root.Children)
			{
				modelBoneInstances.Add(bone);

				foreach (var child in GetChildren(bone))
				{
					modelBoneInstances.Add(child);
				}
			}

			foreach (var bone in modelBoneInstances)
			{
				foreach (var mesh in bone.Meshes)
				{
					modelMeshInstances.Add(mesh);
				}
			}

			var vertexBuffer = new DynamicVertexBuffer(
				Alex.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, vertices.Count,
				BufferUsage.WriteOnly);

			var vertexArray = vertices.ToArray();

			vertexBuffer.SetData(
				0, vertexArray, 0, vertexArray.Length, VertexPositionColorTexture.VertexDeclaration.VertexStride,
				SetDataOptions.None);

			var indexBuffer = new DynamicIndexBuffer(
				Alex.Instance.GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);

			var indexArray = indices.ToArray();
			indexBuffer.SetData(0, indexArray, 0, indexArray.Length, SetDataOptions.None);

			foreach (var meshInstance in modelMeshInstances)
			{
				foreach (var part in meshInstance.MeshParts)
				{
					part.VertexBuffer = vertexBuffer;
					part.IndexBuffer = indexBuffer;
				}
			}

			var mi = new Model(modelBoneInstances, modelMeshInstances);
			mi.Root = root;

			mi.BuildHierarchy();
			instance = mi;

			return true;
		}

		private static ModelBone ProcessBone(EntityModel source,
			EntityModelBone bone,
			ref List<VertexPositionColorTexture> vertices,
			ref List<short> indices)
		{
			ModelBone modelBone = new ModelBone() { Name = bone.Name, Box = new BoundingBox() };

			modelBone.Pivot = bone.Pivot.HasValue ?
				new Vector3(bone.Pivot.Value.X, bone.Pivot.Value.Y, bone.Pivot.Value.Z) : null;

			if (bone.Rotation.HasValue)
			{
				var r = bone.Rotation.Value;
				modelBone.BaseRotation = new Vector3(r.X, r.Y, r.Z);
			}

			if (bone.BindPoseRotation.HasValue)
			{
				var r = bone.BindPoseRotation.Value;
				//	modelBone.BaseRotation += new Vector3(r.X, r.Y, r.Z);
			}

			if (bone.Cubes != null)
			{
				List<ModelMeshPart> meshParts = new List<ModelMeshPart>();

				foreach (var cube in bone.Cubes)
				{
					int vertexStart = vertices.Count;
					int cubeStartIndex = indices.Count;

					if (cube == null)
					{
						continue;
					}

					var inflation = (float)(cube.Inflate ?? bone.Inflate);
					var mirror = cube.Mirror ?? bone.Mirror;

					var origin = cube.InflatedOrigin(inflation);

					/*if (cube.Pivot == null)
					{
						var bonePivot = bone.Pivot;

						if (bonePivot.HasValue)
						{
							var original = origin;
							origin.Z += cube.Size.Z;
							
						}
					}*/

					var modifier = new Vector3(origin.X, origin.Y, origin.Z);

					var bonePivotPoint = bone.Pivot;

					/*if (bonePivotPoint != null)
					{
						if (modifier.X < 0f)
						{
							var piv = bonePivotPoint.Value;

							if (piv.X < 0f)
							{
								modifier.X = MathF.Abs(modifier.X) - (cube.Size.X);
								piv.X =  MathF.Abs(piv.X);

								bonePivotPoint = piv;
							}
						}
						else if (modifier.X > 0f)
						{
							var piv = bonePivotPoint.Value;

							if (piv.X > 0f)
							{
								modifier.X = -(modifier.X + (cube.Size.X));
								piv.X = -piv.X;

								bonePivotPoint = piv;
							}
						}
					}*/

					//origin.Y -= origin.Y;	
					Cube built = new Cube(cube, mirror, inflation, modifier);

					Matrix matrix = Matrix.Identity;

					if (bone.BindPoseRotation.HasValue && bonePivotPoint.HasValue)
					{
						//	bonePivotPoint *= new Vector3(-1f, 1f, 1f);
						var r = bone.BindPoseRotation.Value;

						matrix *= Matrix.CreateTranslation(-bonePivotPoint.Value)
						          * MatrixHelper.CreateRotationDegrees(new Vector3(r.X, r.Y, r.Z))
						          * Matrix.CreateTranslation(bonePivotPoint.Value);
					}

					if (cube.Rotation.HasValue)
					{
						var rotation = cube.Rotation.Value;
						Vector3 pivot = cube.Pivot.GetValueOrDefault(bone.Pivot.GetValueOrDefault(Vector3.Zero));

						//	pivot *= new Vector3(-1f, 1f, 1f);
						//if (cube.Pivot.HasValue)
						{
							//pivot = cube.InflatedPivot(inflation);
							matrix = Matrix.CreateTranslation(-pivot) * MatrixHelper.CreateRotationDegrees(rotation)
							                                          * Matrix.CreateTranslation(pivot);
						}
						//else
						//{
						//pivot = cube.InflatedSize(inflation) / 2f;
						//	matrix = MatrixHelper.CreateRotationDegrees(rotation);
						//}
					}

					//	matrix *= Matrix.CreateTranslation(origin);

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

					//var meshVertices = vertices.GetRange(cubeStartIndex - 1, (vertices.Count - vertexStart));

					var verts = built.Front.vertices.Concat(built.Back.vertices).Concat(built.Top.vertices)
					   .Concat(built.Bottom.vertices).Concat(built.Left.vertices).Concat(built.Right.vertices);

					modelBone.Box = BoundingBox.CreateMerged(
						modelBone.Box, BoundingBox.CreateFromPoints(verts.Select(x => x.Position)));

					meshParts.Add(part);
				}

				var meshInstance = new ModelMesh(Alex.Instance.GraphicsDevice, meshParts) { Name = "Cubes" };

				modelBone.AddMesh(meshInstance);
			}

			modelBone.Visible = !bone.NeverRender;


			foreach (var childBone in source.Bones.Where(
				         x => x.Parent != null && string.Equals(x.Parent, bone.Name, DefaultComparison)))
			{
				if (childBone.Parent != null && childBone.Parent.Equals(childBone.Name, DefaultComparison))
					continue;

				if (string.IsNullOrWhiteSpace(childBone.Name))
					childBone.Name = Guid.NewGuid().ToString();

				var child = ProcessBone(source, childBone, ref vertices, ref indices);
				modelBone.AddChild(child);
			}

			return modelBone;
		}

		private static void ModifyCubeIndexes(ref List<VertexPositionColorTexture> vertices,
			ref List<short> indices,
			(VertexPositionColorTexture[] vertices, short[] indexes) data,
			Matrix transformation)
		{
			var startIndex = vertices.Count;

			for (int i = 0; i < data.vertices.Length; i++)
			{
				var vertex = data.vertices[i];
				var position = Vector3.Transform(vertex.Position, transformation);
				vertices.Add(new VertexPositionColorTexture(position, vertex.Color, vertex.TextureCoordinate));
			}

			for (int i = 0; i < data.indexes.Length; i++)
			{
				var listIndex = startIndex + data.indexes[i];
				indices.Add((short)listIndex);
			}
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
	}
}