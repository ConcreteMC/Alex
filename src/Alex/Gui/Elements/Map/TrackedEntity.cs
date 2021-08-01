using System;
using Alex.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Elements.Map
{
	public class TrackedEntity : MapIcon
	{
		private WeakReference<Entity> _entity;
		public TrackedEntity(Entity entity, MapMarker marker) : base(marker)
		{
			_entity = new WeakReference<Entity>(entity);
		}
	}
}