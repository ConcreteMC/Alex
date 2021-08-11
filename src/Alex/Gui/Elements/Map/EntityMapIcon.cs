using System;
using Alex.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Elements.Map
{
	public class EntityMapIcon : MapIcon
	{
		protected readonly Entity Entity;
		public EntityMapIcon(Entity entity, MapMarker marker) : base(marker)
		{
			Entity = entity;
		}

		/// <inheritdoc />
		public override Vector3 Position
		{
			get
			{
				return Entity.KnownPosition.ToVector3();
			}
		}

		/// <inheritdoc />
		public override float Rotation
		{
			get
			{
				return 180f - Entity.RenderLocation.Yaw;
			}
		}
	}

	public class LocalPlayerMapIcon : EntityMapIcon
	{
		/// <inheritdoc />
		public LocalPlayerMapIcon(Entity entity, MapMarker marker) : base(entity, marker)
		{
			
		}

		/// <inheritdoc />
		public override float Rotation
		{
			get
			{
				return 180f - Entity.RenderLocation.Yaw;
			}
		}
	}
}