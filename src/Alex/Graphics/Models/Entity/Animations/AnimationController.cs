using System.Collections.Generic;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class AnimationController
	{
		public Queue<ModelBoneAnimation> AnimationQueue { get; }
		public AnimationController()
		{
			AnimationQueue = new Queue<ModelBoneAnimation>();	
		}

		public void Update()
		{
			
		}
	}
}