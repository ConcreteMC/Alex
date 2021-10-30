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

			foreach (var bone in definition.Bones)
			{
				_boneComps.TryAdd(bone.Key, new BoneComp(bone.Value));
			}

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

		private double _animationLength = 0d;
		private double _animationTime = 0d;

		public bool CanPlay()
		{
			return true;
		}

		private Dictionary<string, BoneComp> _boneComps = new Dictionary<string, BoneComp>(StringComparer.Ordinal);

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

				if (entity is Bat)
				{
					var a = "b";
				}
				_animationTime = entity.AnimationTime = animTimeUpdate;
				
				foreach (var bone in _boneComps)
				{
					if (bone.Value == null || bone.Value.Bone == null) continue;
					
					bone.Value.Tick(_parent.Runtime, _elapsedTimer.Elapsed.TotalSeconds, _animationTime, anim.OverridePreviousAnimation);
				}
			}
			finally
			{
				_elapsedTimer.Restart();
			}
		}

		/// <inheritdoc />
		public void UpdateBindings(ModelRenderer renderer)
		{
			foreach (var bone in _boneComps)
			{
				if (renderer.GetBone(bone.Key, out var modelBone))
				{
					bone.Value.Bone = modelBone;
				}
			}
		}

		public void AfterUpdate()
		{
			//Check if animation ended.
			if (_animationLength > 0 && _animationTime >= _animationLength)
			{
				Stop();
				if (_loop)
				{
					Play();
				}
			}

		}

		public void Play()
		{
			if (Playing)
				return;
			
			if (_parent?.Entity is Player)
			{
				//Log.Info($"Started animation: {_animName}");
			}

			foreach (var bone in _boneComps)
			{
				bone.Value.Start();
			}
			
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
			if (Playing)
			{
				if (_parent?.Entity is Player)
				{
				//	Log.Info($"Stopped animation: {_animName}");
				}
				
				foreach (var bone in _boneComps)
				{
					bone.Value.Stop();
				}
			}
			Playing = false;
			_animationTime = 0d;
			_elapsedTimer.Stop();
		}
	}

	public class BoneComp
	{
		private readonly AnimationBoneElement _element;
		//private Vector3 _rotation = Vector3.Zero;
		//private Vector3 _position = Vector3.Zero;
		//private Vector3 _scale = Vector3.Zero;

		public ModelBone Bone { get; set; }

		public BoneComp(AnimationBoneElement element)
		{
			_element = element;
			Bone = null;
		}

		private Vector3 _startRotation = Vector3.Zero;
		private Vector3 _startPosition = Vector3.Zero;
		private Vector3 _startScale = Vector3.Zero;
		public void Start()
		{
			var bone = Bone;

			if (bone == null)
				return;

			_startRotation = bone.Rotation;
			_startPosition = bone.Position;
			_startScale = bone.Scale;
		}

		public void Stop()
		{
			var bone = Bone;

			if (bone == null)
				return;

			bone.Rotation = _startRotation;
			bone.Position = _startPosition;
			bone.Scale = _startScale;
		}
		
		public void Tick(MoLangRuntime runtime, double elapsedTime, double animationTime, bool overrideOthers)
		{
			var bone = Bone;

			if (bone == null)
				return;

			var value = _element;

			double rotationTime = elapsedTime;

			var targetRotation =
				value.Rotation?.Evaluate(runtime, Vector3.Zero, out rotationTime, animationTime)
				 ?? Vector3.Zero;
			
			bone.RotateOverTime(targetRotation, rotationTime > 0 ? rotationTime : elapsedTime, overrideOthers);

			double positioningTime = elapsedTime;
			var targetPosition = value.Position?.Evaluate(runtime, Vector3.Zero, out positioningTime, animationTime)
			                     ?? Vector3.Zero;

			bone.TranslateOverTime(targetPosition, positioningTime > 0 ? positioningTime : elapsedTime, overrideOthers);

			double scaleTime = elapsedTime;
			var targetScale = value.Scale?.Evaluate(runtime, bone.Scale, out scaleTime, animationTime) ?? Vector3.One;
			bone.ScaleOverTime(targetScale, scaleTime > 0 ? scaleTime : elapsedTime, true);
		}
	}
}