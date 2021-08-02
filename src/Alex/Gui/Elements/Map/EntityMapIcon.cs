using System;
using Alex.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Elements.Map
{
	public class EntityMapIcon : MapIcon
	{
		private Entity _entity;
		public EntityMapIcon(Entity entity, MapMarker marker) : base(marker)
		{
			_entity = entity;
		}

		/// <inheritdoc />
		public override Vector3 Position
		{
			get
			{
				return _entity.KnownPosition.ToVector3();
			}
		}

		/// <inheritdoc />
		public override float Rotation
		{
			get
			{
				return 180f - _entity.KnownPosition.HeadYaw;
			}
		}
	}
}