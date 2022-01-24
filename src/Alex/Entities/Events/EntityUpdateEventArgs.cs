using System;
using Alex.Common.Utils;

namespace Alex.Entities.Events;

public class EntityUpdateEventArgs : EventArgs
{
	public Entity Entity { get; }
	public TimingsReport TimingsReport { get; }

	public EntityUpdateEventArgs(Entity entity, TimingsReport report)
	{
		Entity = entity;
		TimingsReport = report;
	}
}