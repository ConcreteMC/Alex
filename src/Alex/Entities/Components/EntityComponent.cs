using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Components
{
	public abstract class EntityComponent : IEntityComponent
	{
		public Entity Entity { get; }

		/// <inheritdoc />
		public string Name { get; }
		public bool Enabled { get; set; } = true;

		protected EntityComponent(Entity entity, string name)
		{
			Entity = entity;
			Name = name;
		}
	}

	public abstract class EntityComponentUpdatable : EntityComponent, IUpdated
	{
		protected EntityComponentUpdatable(Entity entity, string name) : base(entity, name) { }

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