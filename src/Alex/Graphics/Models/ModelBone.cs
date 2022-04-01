using System.Collections.Generic;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using NLog;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Alex.Graphics.Models
{
	public class ModelBone
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ModelBone));

		private List<ModelMesh> _meshes = new List<ModelMesh>();

		public List<ModelMesh> Meshes
		{
			get
			{
				return this._meshes;
			}
			private set
			{
				_meshes = value;
			}
		}

		/// <summary>
		///		 Gets a collection of bones that are children of this bone.
		/// </summary>
		public ModelBoneCollection Children { get; private set; }

		/// <summary>
		///		Gets the index of this bone in the Bones collection.
		/// </summary>
		public int Index { get; set; } = -1;

		/// <summary>
		///  Gets the name of this bone.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///		Gets the parent of this bone.
		/// </summary>
		public ModelBone Parent { get; set; }

		/// <summary>
		///		The root model
		/// </summary>
		public Model Model { get; set; }


		/// <summary>
		///		Gets or sets the matrix used to transform this bone relative to its parent bone.
		/// </summary>
		public Matrix Transform { get; set; } = Matrix.Identity;

		/// <summary>
		/// Transform of this node from the root of the model not from the parent
		/// </summary>
		public Matrix ModelTransform
		{
			get;
			set;
		} = Matrix.Identity;

		private Vector3 _baseRotation = Vector3.Zero;

		public Vector3 BaseRotation
		{
			get => _baseRotation;
			set
			{
				_baseRotation = value;
				//UpdateTransform();
			}
		}

		private Vector3 _basePosition = Vector3.Zero;

		public Vector3 BasePosition
		{
			get => _basePosition;
			set
			{
				_basePosition = value;
				//UpdateTransform();
			}
		}

		private Vector3 _baseScale = Vector3.One;

		public Vector3 BaseScale
		{
			get => _baseScale;
			set
			{
				_baseScale = value;
				//UpdateTransform();
			}
		}

		private Vector3? _pivot = null;

		public Vector3? Pivot
		{
			get => _pivot;
			set
			{
				_pivot = value;
				//UpdateTransform();
			}
		}

		public bool Visible { get; set; } = true;

		public ModelBone()
		{
			Children = new ModelBoneCollection(new List<ModelBone>());
		}

		public BoundingBox Box { get; set; }

		public void AddMesh(ModelMesh mesh)
		{
			mesh.ParentBone = this;
			_meshes.Add(mesh);
		}

		public void AddChild(ModelBone modelBone)
		{
			modelBone.Parent = this;
			Children.Add(modelBone);
		}

		public void RemoveChild(ModelBone modelBone)
		{
			if (modelBone.Parent != this)
				return;

			modelBone.Parent = null;
			Children.Remove(modelBone);
		}

		public Matrix GetTransform(BoneMatrices data)
		{
			var box = Box.GetDimensions();
			var pivot = Pivot.GetValueOrDefault(box / 2f);

			return Matrix.CreateScale(_baseScale * data.Scale) * Matrix.CreateTranslation(-pivot)
			                                                   * MatrixHelper.CreateRotationDegrees(
				                                                   _baseRotation + data.Rotation)
			                                                   * Matrix.CreateTranslation(pivot)
			                                                   * Matrix.CreateTranslation(
				                                                   _basePosition + data.Position);
		}
	}
}