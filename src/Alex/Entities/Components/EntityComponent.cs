using System;
using System.Diagnostics;
using Alex.Common.World;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Components
{
	public abstract class EntityComponent : IEntityComponent
	{
		public Entity Entity { get; }

		public bool Enabled { get; set; } = true;

		protected EntityComponent(Entity entity)
		{
			Entity = entity;
		}
	}

	public abstract class EntityComponentUpdatable : EntityComponent, IUpdated
	{
		protected EntityComponentUpdatable(Entity entity) : base(entity) { }

		/// <inheritdoc />
		public TimeSpan LastUpdateElapsedTime { get; private set; }

		private Stopwatch _stopwatch = new Stopwatch();

		/// <inheritdoc />
		public virtual void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;

			_stopwatch.Restart();
			OnUpdate(Alex.DeltaTime);
			_stopwatch.Stop();
			LastUpdateElapsedTime = _stopwatch.Elapsed;
		}

		protected abstract void OnUpdate(float deltaTime);
	}

	public interface IUpdated
	{
		TimeSpan LastUpdateElapsedTime { get; }

		void Update(GameTime gameTime);
		//void OnUpdate(float deltaTime);
	}
}