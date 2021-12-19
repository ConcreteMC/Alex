using System;
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
		public virtual void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;
			
			OnUpdate(Alex.DeltaTime);
		}

		protected abstract void OnUpdate(float deltaTime);
	}

	public interface IUpdated
	{
		void Update(GameTime gameTime);
		//void OnUpdate(float deltaTime);
	}
}