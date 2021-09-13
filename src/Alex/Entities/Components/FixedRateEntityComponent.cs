using System;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Components
{
	public abstract class FixedRateEntityComponent : EntityComponentUpdatable
	{
		private       float _frameAccumulator = 0f;
		private       float _targetTime        = 1f / 20f;
		private int _targetFramerate = 20;
		
		protected int TargetFramerate
		{
			get
			{
				return _targetFramerate;
			}
			set
			{
				_targetTime = 1f / value;
				_targetFramerate = value;
			}
		}

		protected FixedRateEntityComponent(Entity entity, int fps = 20) : base(entity)
		{
			TargetFramerate = fps;
		}
		
		/// <inheritdoc />
		public override void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;
			
			var frameTime = (float) Alex.DeltaTime; // / 50;
			_frameAccumulator += frameTime;

			while (_frameAccumulator >= _targetTime)
			{
				OnUpdate(_targetTime);
				
				_frameAccumulator -= _targetTime;
			}
		}
	}
}