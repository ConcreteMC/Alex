using System;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class KeyframeContainer { }

	public class Keyframe
	{
		public Keyframe(int bone, TimeSpan time, Vector3 rotation, Vector3 position, Vector3 scale)
		{
			Bone = bone;
			Time = time;
			Rotation = rotation;
			Position = position;
			Scale = scale;
			//Transform = transform;
		}

		public int Bone { get; private set; }
		public TimeSpan Time { get; private set; }
		public Vector3 Rotation { get; }
		public Vector3 Position { get; }
		public Vector3 Scale { get; }

		public Matrix Transform => Matrix.CreateScale(Scale) * MatrixHelper.CreateRotationDegrees(Rotation)
		                                                     * Matrix.CreateTranslation(Position);
	}
}