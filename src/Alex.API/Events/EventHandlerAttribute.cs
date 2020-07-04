using System;
using JetBrains.Annotations;

namespace Alex.API.Events
{
	[AttributeUsage(AttributeTargets.Method)]
	public class EventHandlerAttribute : Attribute
	{
		public EventPriority Priority       { get; }
		public bool          IgnoreCanceled { get; }

		public EventHandlerAttribute(EventPriority priority, bool ignoreCanceled = false)
		{
			Priority       = priority;
			IgnoreCanceled = ignoreCanceled;
		}

		public EventHandlerAttribute() : this(EventPriority.Normal)
		{
		}
	}
}