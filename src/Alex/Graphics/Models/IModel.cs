using System;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models;

public interface IModel : IDisposable
{
	EventHandler Disposed { get; set; }

	/// <summary>
	/// A collection of <see cref="ModelBone"/> objects which describe how each mesh in the
	/// mesh collection for this model relates to its parent mesh.
	/// </summary>
	ModelBoneCollection Bones { get; }

	/// <summary>
	/// A collection of <see cref="ModelMesh"/> objects which compose the model. Each <see cref="ModelMesh"/>
	/// in a model may be moved independently and may be composed of multiple materials
	/// identified as <see cref="ModelMeshPart"/> objects.
	/// </summary>
	ModelMeshCollection Meshes { get; }

	/// <summary>
	/// Root bone for this model.
	/// </summary>
	ModelBone Root { get; set; }

	/// <summary>
	/// Custom attached object.
	/// </summary>
	/// <remarks>
	/// Skinning data is example of attached object for model.
	/// </remarks>
	object Tag { get; set; }

	int Draw(Matrix world, Matrix view, Matrix projection, Matrix[] matrices);

	int Draw(Matrix world,
		Matrix view,
		Matrix projection,
		Matrix[] matrices,
		Microsoft.Xna.Framework.Graphics.Effect effect);
}