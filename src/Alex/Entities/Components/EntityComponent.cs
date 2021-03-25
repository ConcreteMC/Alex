using System;
using Alex.API.World;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Components
{
	public abstract class EntityComponent : IEntityComponent
	{
		protected Entity Entity { get; }
		
		public bool Enabled { get; set; } = true;
		protected EntityComponent(Entity entity)
		{
			Entity = entity;
		}

		/// <inheritdoc />
		public virtual void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;
			
			OnUpdate((float) gameTime.ElapsedGameTime.TotalSeconds);
		}

		protected abstract void OnUpdate(float deltaTime);
	}

	public interface ITickedEntityComponent : IEntityComponent, ITicked
	{

	}
}