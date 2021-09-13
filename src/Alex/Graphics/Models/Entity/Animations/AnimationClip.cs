using System;
using System.Collections.Generic;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class AnimationClip
	{
		public TimeSpan Duration { get; }
		public List<Keyframe> Keyframes { get; }

		public AnimationClip(TimeSpan duration, List<Keyframe> keyframes)
		{
			Duration = duration;
			Keyframes = keyframes;
		}
	}
}