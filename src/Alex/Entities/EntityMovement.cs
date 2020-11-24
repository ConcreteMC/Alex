using System.Numerics;
using Alex.API.Utils;

namespace Alex.Entities
{
	/// <summary>
	/// 	TODO: Implement interpolated movement by seperating KnownPosition from RenderPosition
	/// </summary>
	public class EntityMovement
	{
		public Entity Entity { get; }
		public EntityMovement(Entity entity)
		{
			Entity = entity;
		}
		
		public void MoveTo(PlayerLocation location)
		{
			var distance = Microsoft.Xna.Framework.Vector3.Distance(
				Entity.KnownPosition.ToVector3(), location.ToVector3());
			
			Entity.KnownPosition = location;

			Entity.DistanceMoved += MathF.Abs(distance);
		}

		public void Move(Vector3 direction)
		{
			var   oldPosition = Entity.KnownPosition.ToVector3();
			float offset      = 0f;

			//TODO: Fix position offset

			Entity.KnownPosition.X += direction.X;
			Entity.KnownPosition.Y += (direction.Y - offset);
			Entity.KnownPosition.Z += direction.Z;

			Entity.DistanceMoved +=
				MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(oldPosition, Entity.KnownPosition.ToVector3()));
		}

		public void Teleport(PlayerLocation location)
		{
			Entity.KnownPosition = location;
			/*
			var oldPosition = entity.KnownPosition;
			entity.KnownPosition = position;
			//if (entity is PlayerMob p)
			{
				entity.DistanceMoved += MathF.Abs(Vector3.Distance(oldPosition, position));
			}*/
		}
	}
}