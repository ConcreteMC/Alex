using System;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class ServerAnimation : ModelBoneAnimation
	{
		private double Duration { get; }
		private ModelParameters Target { get; }
		private ModelParameters Start { get; }
		private bool ResetAfter { get; }
		
		/// <inheritdoc />
		public ServerAnimation(EntityModelRenderer.ModelBone bone, ModelParameters initial, ModelParameters target, TimeSpan duration, bool resetAfter) : base(bone)
		{
			Start = initial;
			Duration = duration.TotalSeconds;
			Target = target;
			ResetAfter = resetAfter;

			_finished = false;
		}

		/// <inheritdoc />
		protected override void SetupAnimation()
		{
			base.SetupAnimation();

			Bone.Rotation = Start.Rotation;
		}

		private bool _finished = false;
		private float _elapsed = 0f;
		protected override void OnTick(GameTime gameTime, float delta)
		{
			//if (_finished)
			//	return;
			
			_elapsed += delta;
			
			var progress = (1f / Duration) * _elapsed;
			
			if (progress >= 1f)
			{
				_finished = true;
				return;
			}

			Bone.Rotation = new Vector3(EasingFunction.Linear(Start.Rotation.X, Target.Rotation.X, (float)progress)
				, EasingFunction.Linear(Start.Rotation.Y, Target.Rotation.Y, (float)progress)
				, EasingFunction.Linear(Start.Rotation.Z, Target.Rotation.Z, (float)progress));
		}

		/// <inheritdoc />
		public override bool IsFinished()
		{
			return _finished;
		}

		/// <inheritdoc />
		public override void Reset()
		{
			if (ResetAfter)
			{
				Bone.Rotation = Bone.EntityModelBone.Rotation;
				//	base.Reset();
			}
		}
	}
}