using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Graphics.Models.Entity.Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Graphics.Models
{
	/// <summary>
	/// A basic 3D model with per mesh parent bones.
	/// </summary>
	public sealed class Model
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Model));
		private Matrix[] _matrices;

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
			
			foreach(var node in this.Root.Children)
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

		/// <summary>
		/// Draws the model meshes.
		/// </summary>
		/// <param name="world">The world transform.</param>
		/// <param name="view">The view transform.</param>
		/// <param name="projection">The projection transform.</param>
		public int Draw(Matrix world, Matrix view, Matrix projection)
		{
			int drawCount = 0;
			int boneCount = this.Bones.Count;
			
			if (_matrices == null ||
			    _matrices.Length != boneCount)
			{
				_matrices = new Matrix[boneCount];
			}

			// Look up combined bone matrices for the entire model.            
			CopyAbsoluteBoneTransformsTo(_matrices);

			// Draw the model.
			foreach (var mesh in Meshes)
			{
				if (!mesh.ParentBone.Visible)
					continue;
				
				foreach (Microsoft.Xna.Framework.Graphics.Effect effect in mesh.Effects)
				{
					IEffectMatrices effectMatricies = effect as IEffectMatrices;
					if (effectMatricies == null) {
						throw new InvalidOperationException();
					}
					effectMatricies.World = _matrices[mesh.ParentBone.Index] * world;
					effectMatricies.View = view;
					effectMatricies.Projection = projection;
				}

				drawCount += mesh.Draw();
			}

			return drawCount;
		}

		/// <summary>
		/// Copies bone transforms relative to all parent bones of the each bone from this model to a given array.
		/// </summary>
		/// <param name="destinationBoneTransforms">The array receiving the transformed bones.</param>
		public void CopyAbsoluteBoneTransformsTo(Matrix[] destinationBoneTransforms)
		{
			if (destinationBoneTransforms == null)
				throw new ArgumentNullException(nameof(destinationBoneTransforms));
			
			if (destinationBoneTransforms.Length < this.Bones.Count)
				throw new ArgumentOutOfRangeException(nameof(destinationBoneTransforms));
			
			int count = this.Bones.Count;
			for (int index1 = 0; index1 < count; index1++)
			{
				var modelBone = (this.Bones)[index1];
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
	}
}