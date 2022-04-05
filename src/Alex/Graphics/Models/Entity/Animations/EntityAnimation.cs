using System.Diagnostics;
using System.Linq;
using Alex.Interfaces;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using ConcreteMC.MolangSharp.Runtime;
using ConcreteMC.MolangSharp.Runtime.Exceptions;
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

		public bool HoldOnLastFrame { get; set; } = false;
		private double _startDelay = -1;
		private double _loopDelay = -1;
		private bool _isFirstLoop = true;
		public EntityAnimation(AnimationComponent parent, Animation definition, string name)
		{
			_parent = parent;
			_definition = definition;

			_animationLength = definition.AnimationLength;

			_components = definition.Bones.Select(x => new BoneComp(x.Key, this, x.Value)).ToArray();

			try
			{
				if (definition.Loop != null)
				{
					var loop = definition.Loop;

					if (loop == "hold_on_last_frame")
					{
						HoldOnLastFrame = true;
						_loop = false;
					}
					else
					{
						if (bool.TryParse(loop, out _loop))
						{
							
						}
					}
				}
				else
				{
					_loop = false;
				}
			}
			catch (MoLangRuntimeException ex)
			{
				Log.Warn(ex, $"Error while initializing entity animation!");
				_loop = false;
			}
		}

		public bool Playing { get; private set; } = false;
		public double AnimationLength => _animationLength;
		private double _animationLength = 0d;
		private double _animationTime = 0d;

		public bool CanPlay()
		{
			if (HoldOnLastFrame && !_isFirstLoop)
				return false;
			
			return true;
		}

		private BoneComp[] _components;

		private Stopwatch _elapsedTimer = new Stopwatch();

		public void Tick()
		{
			if (!Playing)
				return;

			if (_startDelay > 0d)
			{
				if (_timeSinceStart.Elapsed.TotalSeconds < _startDelay)
					return;
			}

			if (_loopDelay > 0d && !_isFirstLoop)
			{
				if (_timeSinceLastLoop.Elapsed.TotalSeconds < _loopDelay)
					return;
			}
			
			try
			{
				var entity = _parent.Entity;
				var anim = _definition;

				entity.AnimationTime = _animationTime;
				double animTimeUpdate = _animationTime; // + elapsed.TotalSeconds;

				if (anim.AnimationTimeUpdate != null)
				{
					animTimeUpdate = _parent.Execute(anim.AnimationTimeUpdate).AsDouble();
				}

				_animationTime = entity.AnimationTime = animTimeUpdate;

				foreach (var bone in _components)
				{
					if (bone == null || bone.Bone == null) continue;

					bone.Tick(
						_parent.Runtime, _elapsedTimer.Elapsed.TotalSeconds, _animationTime,
						false);
				}
			}
			finally
			{
				_elapsedTimer.Restart();
			}

			if (_animationLength > 0 && _animationTime >= _animationLength)
			{
				_isFirstLoop = false;
				if (_loop)
				{
					_animationTime = 0;

					if (_definition.LoopDelay != null)
					{
						var loopDelay = _parent.Execute(_definition.LoopDelay);
						_loopDelay = loopDelay.AsDouble();
					}
					_timeSinceLastLoop.Restart();
				}
				else
				{
					Stop();
				}
			}
		}

		/// <inheritdoc />
		public void UpdateBindings(ModelRenderer renderer)
		{
			foreach (var bone in _components)
			{
				if (renderer.GetBoneTransform(bone.Name, out var modelBone))
				{
					bone.Bone = modelBone;
				}
			}
		}
		
		public void Play()
		{
			if (Playing)
				return;
			
			if (_definition.StartDelay != null)
			{
				var startDelay = _parent.Execute(_definition.StartDelay);
				_startDelay = startDelay.AsDouble();
			}
			
			_timeSinceStart.Restart();
			foreach (var bone in _components)
			{
				bone.Start(_definition.OverridePreviousAnimation);
			}

			Playing = true;
			_animationTime = 0d;
			_elapsedTimer.Restart();
		}

		private Stopwatch _timeSinceLastLoop = new Stopwatch();
		private Stopwatch _timeSinceStart = new Stopwatch();
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
		public BoneMatrices Bone { get; set; }

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

		public void Start(bool reset = false)
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

			//bone.RotateOverTime(_startRotation, Alex.DeltaTime, true);
			//bone.TranslateOverTime(_startPosition, Alex.DeltaTime, true);
			//bone.ScaleOverTime(_startScale, Alex.DeltaTime, true);
			bone.Rotation = _startRotation;
			bone.Position = _startPosition;
			bone.Scale = _startScale;

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
				var targetRotation = value.Rotation.Evaluate(
					runtime, Primitives.Factory.Vector3Zero, _animation.AnimationLength, animationTime, true);

				bone.RotateOverTime(targetRotation, elapsedTime, overrideOthers);
			}


			if (value.Position != null)
			{
				var targetPosition = value.Position.Evaluate(
					runtime, Primitives.Factory.Vector3Zero, _animation.AnimationLength, animationTime);

				bone.TranslateOverTime(targetPosition, elapsedTime, overrideOthers);
			}

			if (value.Scale != null)
			{
				var targetScale = value.Scale.Evaluate(runtime, Primitives.Factory.Vector3(bone.Scale.X, bone.Scale.Y, bone.Scale.Z), _animation.AnimationLength, animationTime);

				if (targetScale.X < 0.001d)
					targetScale.X = 0;

				if (targetScale.Y < 0.001d)
					targetScale.Y = 0;

				if (targetScale.Z < 0.001d)
					targetScale.Z = 0;

				bone.ScaleOverTime(targetScale, elapsedTime, true);
			}
		}
	}
}