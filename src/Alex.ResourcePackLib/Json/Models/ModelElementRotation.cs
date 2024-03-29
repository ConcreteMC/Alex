﻿using Alex.Interfaces;

namespace Alex.ResourcePackLib.Json.Models
{
	public class ModelElementRotation
	{
		/// <summary>
		/// Sets the center of the rotation according to the scheme [x, y, z], defaults to [8, 8, 8].
		/// </summary>
		public IVector3 Origin { get; set; } = Primitives.Factory.Vector3(8, 8, 8);

		/// <summary>
		/// Specifies the direction of rotation, can be "x", "y" or "z".
		/// </summary>
		public Axis Axis { get; set; } = Axis.Undefined;

		/// <summary>
		/// Specifies the angle of rotation. Can be 45 through -45 degrees in 22.5 degree increments. Defaults to 0.
		/// </summary>
		public double Angle { get; set; } = 0;

		/// <summary>
		/// Specifies whether or not to scale the faces across the whole block. Can be true or false. Defaults to false.
		/// </summary>
		public bool Rescale { get; set; } = false;

		public ModelElementRotation Clone()
		{
			return new ModelElementRotation()
			{
				Angle = Angle, Axis = Axis, Origin = Primitives.Factory.Vector3(Origin.X, Origin.Y, Origin.Z), Rescale = Rescale
			};
		}
	}
}