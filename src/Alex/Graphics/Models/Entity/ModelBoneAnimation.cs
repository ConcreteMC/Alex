using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity
{
	public class ModelBoneAnimation
	{
		protected EntityModelRenderer.ModelBone Bone { get; }
		protected ModelParameters Initial { get; set; }
		
		public ModelBoneAnimation(EntityModelRenderer.ModelBone bone)
		{
			Bone = bone;
			
		}

		private bool _isSetup = false;
		public void Setup()
		{
			if (_isSetup)
				return;

			_isSetup = true;
			
			Initial = new ModelParameters(Bone.Rotation, Bone.Position);
			
			SetupAnimation();
		}

		protected virtual void SetupAnimation()
		{
			
		}

		private Stopwatch RuntimeStopwatch { get; } = new Stopwatch();
		protected TimeSpan ElapsedTime => RuntimeStopwatch.Elapsed;
		public void Start()
		{
			RuntimeStopwatch.Restart();
		}
		
		protected virtual void OnStart(){}
		
		public void Update(GameTime gameTime)
		{
			if (!_isSetup)
				return;
			
			OnTick(gameTime, (float) gameTime.ElapsedGameTime.TotalSeconds);
		}

		protected virtual void OnTick(GameTime gameTime, float delta)
		{
			
		}

		public virtual bool CanStart()
		{
			return true;
		}

		public virtual bool IsFinished()
		{
			return true;
		}

		public virtual void Reset()
		{
			Initial.Apply(Bone);
		}
	}
	
	public class ModelParameters
	{
		public Vector3 Rotation { get; set; }
		public Vector3 Position { get; set; }

		public ModelParameters(Vector3 rotation, Vector3 position)
		{
			Rotation = rotation;
			Position = position;
		}

		public void Apply(EntityModelRenderer.ModelBone bone)
		{
			bone.Rotation = Rotation;
			bone.Position = Position;
		}
	}
}