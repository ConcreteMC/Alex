using System.Collections.Generic;
using Alex.Interfaces;

namespace Alex.ResourcePackLib.Json.Models
{
	public class ModelElement
	{
		/// <summary>
		/// Start point of a cube according to the scheme [x, y, z]. Values must be between -16 and 32.
		/// </summary>
		public IVector3 From { get; set; } = Primitives.Factory.Vector3(0, 0, 0);

		/// <summary>
		/// Stop point of a cube according to the scheme [x, y, z]. Values must be between -16 and 32.
		/// </summary>
		public IVector3 To { get; set; } = Primitives.Factory.Vector3(16, 16, 16);

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
		public Dictionary<BlockFace, ModelElementFace>
			Faces { get; set; } // = new Dictionary<BlockFace, BlockModelElementFace>();

		public ModelElement Clone()
		{
			Dictionary<BlockFace, ModelElementFace> faces = new Dictionary<BlockFace, ModelElementFace>();

			var clone = new ModelElement()
			{
				From = Primitives.Factory.Vector3(From.X, From.Y, From.Z),
				To = Primitives.Factory.Vector3(To.X, To.Y, To.Z),
				Rotation = Rotation.Clone(),
				Shade = Shade
			};

			if (Faces != null)
			{
				foreach (var f in Faces)
				{
					faces.Add(f.Key, f.Value.Clone());
				}
			}

			clone.Faces = faces;

			return clone;
		}
	}
}