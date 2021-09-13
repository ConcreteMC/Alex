using System;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class Keyframe
	{
		public Keyframe(int bone, TimeSpan time, Matrix transform)
		{
			Bone = bone;
			Time = time;
			Transform = transform;
		}
		
		public int Bone { get; private set; }
		public TimeSpan Time { get; private set; }
		public Matrix Transform { get; private set; }
	}
}