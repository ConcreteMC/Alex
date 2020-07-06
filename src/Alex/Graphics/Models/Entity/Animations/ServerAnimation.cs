using System;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using MathF = System.MathF;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class ServerAnimation : ModelBoneAnimation
	{
		private double Duration { get; }
		private ModelParameters Target { get; }
		
		/// <inheritdoc />
		public ServerAnimation(EntityModelRenderer.ModelBone bone, ModelParameters initial, ModelParameters target, TimeSpan duration) : base(bone)
		{
			Duration = duration.TotalSeconds;
			Target = target;
		}
		
		private bool _finished = false;
		private float _elapsed = 0f;
		protected override void OnTick(GameTime gameTime, float delta)
		{
			_elapsed += delta;
			var progress = (1f / Duration) * _elapsed;

			//float floatProgress = progress;
			
			Bone.Rotation = new Vector3(EasingFunction.Linear(Initial.Rotation.X, Target.Rotation.X, (float)progress)
				, EasingFunction.Linear(Initial.Rotation.Y, Target.Rotation.Y, (float)progress)
				, EasingFunction.Linear(Initial.Rotation.Z, Target.Rotation.Z, (float)progress));
			
			if (progress >= 1f)
				_finished = true;
			
			//Bone.Rotation = new Vector3(EasingFunction.EaseInCubic(Initial.Rotation.X, 120f, prog), 0f, (MathF.Sin(_swingValue2) * 17.5f));
		}

		/// <inheritdoc />
		public override bool IsFinished()
		{
			return _finished;
		}
	}
}