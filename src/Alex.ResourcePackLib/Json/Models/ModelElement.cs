using System.Collections.Generic;
using Alex.API.Blocks;
using Microsoft.Xna.Framework;

namespace Alex.ResourcePackLib.Json.Models
{
	public class ModelElement
	{
		/// <summary>
		/// Start point of a cube according to the scheme [x, y, z]. Values must be between -16 and 32.
		/// </summary>
		public Vector3 From { get; set; } = new Vector3(0, 0, 0);

		/// <summary>
		/// Stop point of a cube according to the scheme [x, y, z]. Values must be between -16 and 32.
		/// </summary>
		public Vector3 To { get; set; } = new Vector3(16, 16, 16);

		/// <summary>
		/// Defines the rotation of an element.
		/// </summary>
		public ModelElementRotation Rotation { get; set; } = new ModelElementRotation();

		/// <summary>
		/// Defines if shadows are rendered (true - default), not (false).
		/// </summary>
		public bool Shade { get; set; } = true;

		/// <summary>
		/// Holds all the faces of the cube. If a face is left out, it will not be rendered.
		/// </summary>
		public IReadOnlyDictionary<BlockFace, ModelElementFace> Faces { get; set; }// = new Dictionary<BlockFace, BlockModelElementFace>();
	}
}
