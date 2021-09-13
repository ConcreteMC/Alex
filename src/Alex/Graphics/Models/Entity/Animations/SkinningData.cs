using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class SkinningData
	{
		public Dictionary<string, AnimationClip> AnimationClips { get; }
		public List<Matrix> BindPose { get; }
		public List<Matrix> InverseBindPose { get; }
		public List<int> SkeletonHierarchy { get; }

		public SkinningData(Dictionary<string, AnimationClip> animationClips,
			List<Matrix> bindPose,
			List<Matrix> inverseBindPose,
			List<int> skeletonHierarchy)
		{
			AnimationClips = animationClips;
			BindPose = bindPose;
			InverseBindPose = inverseBindPose;
			SkeletonHierarchy = skeletonHierarchy;
		}
	}
}