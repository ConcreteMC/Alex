using System;
using Alex.API.Utils;
using Alex.Utils;
using Microsoft.Xna.Framework;
using MathF = System.MathF;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class SwingAnimation : ModelBoneAnimation
	{
		public float SwingDuration { get; }
		public SwingAnimation(EntityModelRenderer.ModelBone bone, TimeSpan swingDuration) : base(bone)
		{
			SwingDuration = (float) swingDuration.TotalSeconds;
		}

		private bool _finished = false;
		private float _swingValue = 0f;
		private float _swingValue2 = 0f;

		/// <inheritdoc />
		protected override void OnTick(GameTime gameTime, float delta)
		{
			//_swingValue2 +=
			_swingValue += delta;
			_swingValue2 += delta * 10f;
			var progress = (1f / SwingDuration) * _swingValue;

			var prog = progress;

			if (prog > 1f)
				prog = 2f - prog;
			
			Bone.Rotation = new Vector3(EasingFunction.EaseInCubic(Initial.Rotation.X, 120f, prog), 0f, (MathF.Sin(_swingValue2) * 17.5f));

			if (progress >= 2f)
				_finished = true;
		}
		
		/// <inheritdoc />
		public override bool IsFinished()
		{
			return _finished;
		}
	}
}