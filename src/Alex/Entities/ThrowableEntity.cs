using Alex.Common.Resources;
using Alex.Items;
using Alex.Net;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	public class ThrowableEntity : LivingEntity
	{
		public bool StopOnImpact { get; set; } = false;
		public bool DespawnOnImpact { get; set; } = false;

		/// <inheritdoc />
		public ThrowableEntity(World level) : base(level)
		{
			base.HasPhysics = true;
		}

		/// <inheritdoc />
		public override float CollidedWithWorld(Vector3 direction, Vector3 position, float impactVelocity)
		{
			if (StopOnImpact)
			{
				Velocity = Vector3.Zero;
				NoAi = true;

				return 0;
			}

			return base.CollidedWithWorld(direction, position, impactVelocity);
		}
	}

	public class ThrowableItemEntity : ItemBaseEntity
	{
		private ResourceLocation _item;

		/// <inheritdoc />
		public ThrowableItemEntity(World level, ResourceLocation item) : base(level)
		{
			_item = item;
		}

		/// <inheritdoc />
		public override void OnSpawn()
		{
			base.OnSpawn();

			if (ItemFactory.TryGetItem(_item, out var item))
			{
				SetItem(item);
			}
		}

		/// <inheritdoc />
		public override float CollidedWithWorld(Vector3 direction, Vector3 position, float impactVelocity)
		{
			if (StopOnImpact)
			{
				Velocity = Vector3.Zero;
				NoAi = true;

				return 0;
			}

			return base.CollidedWithWorld(direction, position, impactVelocity);
		}
	}
}