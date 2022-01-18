using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.Entities;
using Alex.Entities.Passive;
using Alex.MoLang.Runtime;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Graphics.Models.Entity.Animations
{
	public interface IAnimation
	{
		void Tick();
		void UpdateBindings(ModelRenderer renderer);
	}

	public class EntityAnimation : IAnimation
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityAnimation));
		private readonly AnimationComponent _parent;
		private readonly Animation _definition;
		private bool _loop;
		
		public EntityAnimation(AnimationComponent parent, Animation definition, string name)
		{
			_parent = parent;
			_definition = definition;

			_animationLength = definition.AnimationLength;

			_components = definition.Bones.Select(x => new BoneComp(x.Key, this, x.Value)).ToArray();

			if (definition.Loop != null)
			{
				_loop = parent.Execute(definition.Loop).AsBool();
			}
			else
			{
				_loop = false;
			}
		}

		public bool Playing { get; private set; } = false;
		public double AnimationLength => _animationLength;
		private double _animationLength = 0d;
		private double _animationTime = 0d;

		public bool CanPlay()
		{
			return true;
		}

		private BoneComp[] _components;

		private Stopwatch _elapsedTimer = new Stopwatch();
		public void Tick()
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
				
				foreach (var bone in _components)
				{
					if (bone == null || bone.Bone == null) continue;
					
					bone.Tick(_parent.Runtime, _elapsedTimer.Elapsed.TotalSeconds, _animationTime, anim.OverridePreviousAnimation);
				}
			}
			finally
			{
				_elapsedTimer.Restart();
			}
			
			if (_animationLength > 0 && _animationTime >= _animationLength)
			{
				Stop();
				if (_loop)
				{
					Play();
				}
			}
		}

		/// <inheritdoc />
		public void UpdateBindings(ModelRenderer renderer)
		{
			foreach (var bone in _components)
			{
				if (renderer.GetBone(bone.Name, out var modelBone))
				{
					bone.Bone = modelBone;
				}
			}
		}

		public void Play()
		{
			if (Playing)
				return;

			foreach (var bone in _components)
			{
				bone.Start();
			}
			
			Playing = true;
			_animationTime = 0d;
			_elapsedTimer.Restart();
		}

		public void Stop()
		{
			if (Playing)
			{
				foreach (var bone in _components)
				{
					bone.Stop();
				}
			}
			Playing = false;
			_animationTime = 0d;
			_elapsedTimer.Stop();
		}
	}

	public class BoneComp
	{
		public string Name { get; }
		private readonly EntityAnimation _animation;

		private readonly AnimationBoneElement _element;
		//private Vector3 _rotation = Vector3.Zero;
		//private Vector3 _position = Vector3.Zero;
		//private Vector3 _scale = Vector3.Zero;

		public ModelBone Bone { get; set; }

		public BoneComp(string name, EntityAnimation animation, AnimationBoneElement element)
		{
			Name = name;
			_animation = animation;
			_element = element;
			Bone = null;
		}

		private bool _started = false;
		private Vector3 _startRotation = Vector3.Zero;
		private Vector3 _startPosition = Vector3.Zero;
		private Vector3 _startScale = Vector3.Zero;
		public void Start()
		{
			if (_started) return;
			
			var bone = Bone;

			if (bone == null)
				return;

			_startRotation = bone.Rotation;
			_startPosition = bone.Position;
			_startScale = bone.Scale;

			_started = true;
		}

		public void Stop()
		{
			if (!_started)
				return;
			
			var bone = Bone;

			if (bone == null)
				return;

			bone.RotateOverTime(_startRotation, Alex.DeltaTime, true);
			bone.TranslateOverTime(_startPosition, Alex.DeltaTime, true);
			bone.ScaleOverTime(_startScale, Alex.DeltaTime, true);
			//bone.Rotation = _startRotation;
			//bone.Position = _startPosition;
			//bone.Scale = _startScale;
			
			_started = false;
		}
		
		public void Tick(MoLangRuntime runtime, double elapsedTime, double animationTime, bool overrideOthers)
		{
			var bone = Bone;

			if (bone == null)
				return;

			var value = _element;

			if (value.Rotation != null)
			{
				var targetRotation = value.Rotation.Evaluate(runtime, Vector3.Zero, _animation.AnimationLength, animationTime, true);

				bone.RotateOverTime(targetRotation, elapsedTime, overrideOthers);
			}


			if (value.Position != null)
			{
				var targetPosition = value.Position.Evaluate(runtime, Vector3.Zero, _animation.AnimationLength, animationTime);

				bone.TranslateOverTime(
					targetPosition, elapsedTime, overrideOthers);
			}

			if (value.Scale != null)
			{
				var targetScale = value.Scale.Evaluate(runtime, bone.Scale, _animation.AnimationLength, animationTime);

				bone.ScaleOverTime(targetScale, elapsedTime, true);
			}
		}
	}
}