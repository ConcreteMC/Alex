using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Graphics.Models
{
	/// <summary>
	/// A basic 3D model with per mesh parent bones.
	/// </summary>
	public sealed class Model : IModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Model));
		//private Matrix[] _matrices;

		/// <inheritdoc />
		public EventHandler Disposed { get; set; }

		/// <summary>
		/// A collection of <see cref="ModelBone"/> objects which describe how each mesh in the
		/// mesh collection for this model relates to its parent mesh.
		/// </summary>
		public ModelBoneCollection Bones { get; private set; }

		/// <summary>
		/// A collection of <see cref="ModelMesh"/> objects which compose the model. Each <see cref="ModelMesh"/>
		/// in a model may be moved independently and may be composed of multiple materials
		/// identified as <see cref="ModelMeshPart"/> objects.
		/// </summary>
		public ModelMeshCollection Meshes { get; private set; }

		/// <summary>
		/// Root bone for this model.
		/// </summary>
		public ModelBone Root { get; set; }

		/// <summary>
		/// Custom attached object.
		/// </summary>
		/// <remarks>
		/// Skinning data is example of attached object for model.
		/// </remarks>
		public object Tag { get; set; }

		/// <summary>
		/// Constructs a model. 
		/// </summary>
		public Model(List<ModelBone> bones, List<ModelMesh> meshes)
		{
			foreach (var bone in bones)
			{
				bone.Model = this;
			}

			Bones = new ModelBoneCollection(bones);
			Meshes = new ModelMeshCollection(meshes);
		}

		internal void BuildHierarchy()
		{
			var globalScale = Matrix.CreateScale(0.01f);

			foreach (var node in this.Root.Children)
			{
				node.Parent = this.Root;
				BuildHierarchy(node, this.Root.Transform * globalScale, 0);
			}
		}

		private void BuildHierarchy(ModelBone node, Matrix parentTransform, int level)
		{
			node.ModelTransform = node.Transform * parentTransform;

			foreach (var child in node.Children)
			{
				child.Parent = node;
				BuildHierarchy(child, node.ModelTransform, level + 1);
			}
		}

		private static ArrayPool<Matrix> MatrixArrayPool = ArrayPool<Matrix>.Create();

		/// <summary>
		/// Draws the model meshes.
		/// </summary>
		/// <param name="world">The world transform.</param>
		/// <param name="view">The view transform.</param>
		/// <param name="projection">The projection transform.</param>
		public int Draw(Matrix world, Matrix view, Matrix projection)
		{
			var bonesCollection = this.Bones;
			var meshes = this.Meshes;

			if (bonesCollection == null || meshes == null)
				return 0;

			var bones = this.Bones.ImmutableArray;
			int boneCount = bones.Length;

			var matrices = MatrixArrayPool.Rent(boneCount);
			CopyAbsoluteBoneTransformsTo(bones, matrices);

			try
			{
				return Draw(world, view, projection, matrices);
			}
			finally
			{
				MatrixArrayPool.Return(matrices, true);
			}
		}

		public int Draw(Matrix world, Matrix view, Matrix projection, Matrix[] matrices)
		{
			if (matrices == null)
			{
				return Draw(world, view, projection);
			}

			return Draw(world, view, projection, matrices, null);
		}

		public int Draw(Matrix world,
			Matrix view,
			Matrix projection,
			Matrix[] matrices,
			Microsoft.Xna.Framework.Graphics.Effect effect)
		{
			var bonesCollection = this.Bones;
			var meshes = this.Meshes;

			if (bonesCollection == null || meshes == null)
				return 0;

			int drawCount = 0;

			var bones = this.Bones.ImmutableArray;
			int boneCount = bones.Length;

			if (matrices.Length < boneCount)
				return 0;

			try
			{
				// Draw the model.
				foreach (var mesh in meshes)
				{
					if (mesh.ParentBone == null || !mesh.ParentBone.Visible || mesh.ParentBone.Index < 0
					    || mesh.ParentBone.Index >= matrices.Length || mesh.Effects == null)
						continue;

					var parentIndex = mesh.ParentBone.Index;

					var setEffectParams = (Microsoft.Xna.Framework.Graphics.Effect eff) =>
					{
						IEffectMatrices effectMatricies = eff as IEffectMatrices;

						if (effectMatricies != null)
						{
							effectMatricies.World = matrices[parentIndex] * world;
							effectMatricies.View = view;
							effectMatricies.Projection = projection;
						}
					};

					if (effect != null)
					{
						setEffectParams(effect);
					}
					else
					{
						foreach (Microsoft.Xna.Framework.Graphics.Effect eff in mesh.Effects)
						{
							setEffectParams(eff);
						}
					}

					foreach (var meshPart in mesh.MeshParts)
					{
						var meshEffect = effect ?? meshPart.Effect;

						if (meshEffect != null)
							drawCount += meshPart.Draw(meshEffect.GraphicsDevice, meshEffect);
					}
				}
			}
			finally { }

			return drawCount;
		}

		/// <summary>
		/// Copies bone transforms relative to all parent bones of the each bone from this model to a given array.
		/// </summary>
		/// <param name="destinationBoneTransforms">The array receiving the transformed bones.</param>
		public void CopyAbsoluteBoneTransformsTo(Matrix[] destinationBoneTransforms)
		{
			CopyAbsoluteBoneTransformsTo(Bones.ToImmutableArray(), destinationBoneTransforms);
		}

		/// <summary>
		/// Copies bone transforms relative to all parent bones of the each bone from this model to a given array.
		/// </summary>
		/// <param name="destinationBoneTransforms">The array receiving the transformed bones.</param>
		public static void CopyAbsoluteBoneTransformsTo(IList<ModelBone> source, Matrix[] destinationBoneTransforms)
		{
			var bones = source;

			if (destinationBoneTransforms == null)
				throw new ArgumentNullException(nameof(destinationBoneTransforms));

			//var bones = this.Bones;
			if (destinationBoneTransforms.Length < bones.Count)
				throw new ArgumentOutOfRangeException(nameof(destinationBoneTransforms));

			int count = bones.Count;

			for (int index1 = 0; index1 < count; index1++)
			{
				if (index1 >= bones.Count)
					break;

				var modelBone = bones[index1];

				if (modelBone.Parent == null)
				{
					destinationBoneTransforms[index1] = modelBone.Transform;
				}
				else
				{
					int parentIndex = modelBone.Parent.Index;

					destinationBoneTransforms[index1] = Matrix.Multiply(
						modelBone.Transform, destinationBoneTransforms[parentIndex]);
				}
			}
		}

		/// <summary>
		/// Copies bone transforms relative to <see cref="Model.Root"/> bone from a given array to this model.
		/// </summary>
		/// <param name="sourceBoneTransforms">The array of prepared bone transform data.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sourceBoneTransforms"/> is null.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="sourceBoneTransforms"/> is invalid.
		/// </exception>
		public void CopyBoneTransformsFrom(Matrix[] sourceBoneTransforms)
		{
			if (sourceBoneTransforms == null)
				throw new ArgumentNullException("sourceBoneTransforms");

			if (sourceBoneTransforms.Length < Bones.Count)
				throw new ArgumentOutOfRangeException("sourceBoneTransforms");

			int count = Bones.Count;

			for (int i = 0; i < count; i++)
			{
				Bones[i].Transform = sourceBoneTransforms[i];
			}
		}

		/// <summary>
		/// Copies bone transforms relative to <see cref="Model.Root"/> bone from this model to a given array.
		/// </summary>
		/// <param name="destinationBoneTransforms">The array receiving the transformed bones.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="destinationBoneTransforms"/> is null.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="destinationBoneTransforms"/> is invalid.
		/// </exception>
		public void CopyBoneTransformsTo(Matrix[] destinationBoneTransforms)
		{
			if (destinationBoneTransforms == null)
				throw new ArgumentNullException("destinationBoneTransforms");

			if (destinationBoneTransforms.Length < Bones.Count)
				throw new ArgumentOutOfRangeException("destinationBoneTransforms");

			int count = Bones.Count;

			for (int i = 0; i < count; i++)
			{
				destinationBoneTransforms[i] = Bones[i].Transform;
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			var instances = _instances;

			if (instances != null && instances.Count == 0)
			{
				_instances = null;
				Disposed?.Invoke(this, EventArgs.Empty);
				var meshes = Meshes;
				Meshes = null;

				foreach (var mesh in meshes)
				{
					mesh?.Dispose();
				}

				var bones = Bones;

				Bones = null;
			}

			if (!disposing)
			{
				Log.Warn($"Model not garbage collected!");
			}
		}

		~Model()
		{
			Dispose(false);
		}

		public IModel Instanced()
		{
			return new InstancedModel(this);
		}

		private List<InstancedModel> _instances = new List<InstancedModel>();

		private void RegisterInstance(InstancedModel instance)
		{
			_instances.Add(instance);
		}

		private void InstanceDestroyed(InstancedModel instance)
		{
			if (_instances.Remove(instance) && _instances.Count == 0)
			{
				Dispose();
			}
		}

		private class InstancedModel : IModel
		{
			private Model _parent;

			public InstancedModel(Model parent)
			{
				_parent = parent;
				parent.RegisterInstance(this);
				parent.Disposed += ParentDisposed;
			}

			private void ParentDisposed(object sender, EventArgs e)
			{
				Disposed?.Invoke(this, EventArgs.Empty);
			}

			/// <inheritdoc />
			public EventHandler Disposed { get; set; }

			/// <inheritdoc />
			public ModelBoneCollection Bones => _parent?.Bones;

			/// <inheritdoc />
			public ModelMeshCollection Meshes => _parent?.Meshes;

			/// <inheritdoc />
			public ModelBone Root
			{
				get
				{
					return _parent?.Root;
				}
				set { }
			}

			/// <inheritdoc />
			public object Tag { get; set; }

			/// <inheritdoc />
			public int Draw(Matrix world, Matrix view, Matrix projection, Matrix[] matrices)
			{
				return _parent?.Draw(world, view, projection, matrices) ?? 0;
			}

			public int Draw(Matrix world,
				Matrix view,
				Matrix projection,
				Matrix[] matrices,
				Microsoft.Xna.Framework.Graphics.Effect effect)
			{
				return _parent?.Draw(world, view, projection, matrices, effect) ?? 0;
			}


			private void Dispose(bool disposing)
			{
				var parent = _parent;

				if (parent != null)
				{
					_parent.Disposed -= ParentDisposed;
					_parent.InstanceDestroyed(this);
					_parent = null;
				}
			}

			/// <inheritdoc />
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			~InstancedModel()
			{
				Log.Warn($"InstancedModel not garbage collected!");
				Dispose(false);
			}
		}
	}
}