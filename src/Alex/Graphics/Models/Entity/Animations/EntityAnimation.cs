using System;
using System.Diagnostics;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.Animations
{
	public interface IAnimation
	{
		void Update();
	}
	
	public class EntityAnimation : IAnimation
	{
		private readonly AnimationComponent _parent;
		private readonly Animation _definition;

		public EntityAnimation(AnimationComponent parent, Animation definition)
		{
			_parent = parent;
			_definition = definition;

			_animationLength = definition.AnimationLength;
		}

		public bool Playing { get; private set; } = true;

		private double _animationLength = 0d;
		private double _animationTime = 0d;

		public void BeforeUpdate()
		{
			
		}
		
		private Stopwatch _elapsedTimer = new Stopwatch();
		public void Update()
		{
			if (!Playing)
				return;
			
			try
			{
				var entity = _parent.Entity;
				var anim = _definition;

				entity.AnimationTime = _animationTime;
				double animTimeUpdate = _animationTime; // + elapsed.TotalSeconds;

				if (anim.AnimationTimeUpdate != null && anim.AnimationTimeUpdate.Length > 0)
				{
					animTimeUpdate = _parent.Execute(anim.AnimationTimeUpdate).AsDouble();
				}

				_animationTime = entity.AnimationTime = animTimeUpdate;

				var renderer = entity.ModelRenderer;

				if (renderer == null || anim.Bones == null)
					return;

				foreach (var bone in anim.Bones)
				{
					if (bone.Value == null) continue;

					if (renderer.GetBone(bone.Key, out var modelBone))
					{
						var value = bone.Value;

						var targetRotation = value.Rotation?.Evaluate(_parent.Runtime, Vector3.Zero, _animationTime)
						                     ?? Vector3.Zero;

						var targetPosition = value.Position?.Evaluate(_parent.Runtime, modelBone.Position, _animationTime)
						                     ?? Vector3.Zero;

						var targetScale = value.Scale?.Evaluate(_parent.Runtime, modelBone.Scale, _animationTime) ?? Vector3.One;

						modelBone.MoveOverTime(
							targetPosition, targetRotation * new Vector3(-1f, 1f, 1f),
							targetScale, _elapsedTimer.Elapsed, anim.OverridePreviousAnimation,
							anim.BlendWeight != null ? _parent.Execute(anim.BlendWeight).AsFloat() : 1f);
					}
				}
			}
			finally
			{
				_elapsedTimer.Restart();
			}
		}

		public void AfterUpdate()
		{
			//Check if animation ended.
			if (_animationLength > 0 && _animationTime >= _animationLength)
			{
				Stop();
				if (_definition.Loop)
				{
					Play();
				}
			}

		}

		public void Play()
		{
			if (Playing)
				return;
			
			Playing = true;
			_animationTime = 0d;
			_elapsedTimer.Restart();
		}

		public void Pause()
		{
			if (!Playing)
				return;

			Playing = false;
			_elapsedTimer.Stop();
		}

		public void Stop()
		{
			Playing = false;
			_animationTime = 0d;
			_elapsedTimer.Stop();
		}
	}
}