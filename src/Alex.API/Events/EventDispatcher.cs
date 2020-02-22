using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Utils;
using NLog;

namespace Alex.API.Events
{
	public class EventDispatcher : IEventDispatcher
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		private static readonly ThreadSafeList<Type> EventTypes = new ThreadSafeList<Type>
		{
			
		};

		static EventDispatcher()
		{
			
		}

		public void RegisterEventType<TEvent>() where TEvent : Event
		{
			Type t = typeof(TEvent);
			if (!RegisterEventType(t))
			{
				throw new DuplicateTypeException();
			}
		}

		public bool RegisterEventType(Type type)
		{
			if (type.IsAbstract)
			{
				return false;
			}
			
			if (RegisteredEvents.ContainsKey(type) || !EventTypes.TryAdd(type))
			{
				return false;
			}
			else
			{
				RegisteredEvents.Add(type, new EventDispatcherValues());
				Log.Info($"Registered event type \"{type.FullName}\"");

				return true;
			}
		}

		public void LoadFrom(Assembly assembly)
		{
			var count = GetEventTypes(assembly).Count(RegisterEventType);
			if (count > 0)
				Log.Info($"Registered {count} event types from assembly {assembly.ToString()}");
		}

		public void Unload(Assembly assembly)
		{
			int count = 0;
			foreach (var eventType in (from eventType in EventTypes
									   where eventType.Assembly == assembly
									   select eventType))
			{
				if (EventTypes.Remove(eventType))
					count++;

				RegisteredEvents.Remove(eventType);
			}

			Log.Info($"Unloaded {count} event types from assembly {assembly.ToString()}");
		}

		private static IEnumerable<Type> GetEventTypes(Assembly assembly)
		{
			return assembly.GetTypes().Where(p =>
			{
				if (p.IsClass && !p.IsAbstract && typeof(Event).IsAssignableFrom(p))
				{
					return true;
				}

				return false;
			});
		}

		private Dictionary<Type, EventDispatcherValues> RegisteredEvents { get; }

		private EventDispatcher[] ExtraDispatchers { get; }

		public EventDispatcher(params EventDispatcher[] dispatchers)
		{
			ExtraDispatchers = dispatchers.Where(x => x != this).ToArray();

			RegisteredEvents = new Dictionary<Type, EventDispatcherValues>();
			foreach (var eventType in EventTypes)
			{
				RegisteredEvents.Add(eventType, new EventDispatcherValues());
			}

			//Log.Info($"Registered {RegisteredEvents.Count} event types!");
		}
		
		public EventDispatcher() : this(new EventDispatcher[]{}){}

		public void RegisterEvents<T>(T obj) where T : class
		{
			int count = 0;

			var type = typeof(Event);
			foreach (var method in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				EventHandlerAttribute attribute = method.GetCustomAttribute<EventHandlerAttribute>(false);
				if (attribute == null) continue;

				var parameters = method.GetParameters();
				if (parameters.Length != 1 || !type.IsAssignableFrom(parameters[0].ParameterType)) continue;

				var paramType = parameters[0].ParameterType;

				EventDispatcherValues e = null;
				if (!RegisteredEvents.TryGetValue(paramType, out e))
				{
					if (EventTypes.TryAdd(paramType))
					{
						e = new EventDispatcherValues();
						RegisteredEvents.Add(paramType, e);
					}
				}

				if (!e.RegisterEventHandler(attribute, obj, method))
				{
					Log.Warn($"Duplicate found for class \"{obj.GetType()}\" of type \"{paramType}\"");
				}
				else
				{
					count++;
				}
			}

			Log.Info($"Registered {count} event handlers for \"{obj}\"");
		}

		public void UnregisterEvents<T>(T obj) where T : class
		{
			foreach (var kv in RegisteredEvents.ToArray())
			{
				kv.Value.Clear(obj);
			}
		}

		private void DispatchPrivate(Event e)
		{
			try
			{
				Type type = e.GetType();
				if (RegisteredEvents.TryGetValue(type, out EventDispatcherValues v))
				{
					v.Dispatch(e);
				}
				else
				{
					Log.Warn($"Unknown event type found! \"{type}\"");
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error while dispatching event: {ex.ToString()}");
			}
		}

		public void DispatchEvent(Event e)
		{
			DispatchPrivate(e);

			if (!e.IsCancelled)
			{
				foreach (var i in ExtraDispatchers)
				{
					i.DispatchPrivate(e);
					if (e.IsCancelled) break;
				}
			}
		}

		/*public async Task<TEvent> DispatchEventAsync<TEvent>(TEvent e) where TEvent : Event
		{
			try
			{
				var type = typeof(TEvent);
				if (RegisteredEvents.TryGetValue(type, out EventDispatcherValues v))
				{
					v.Dispatch(e);
				}
				else
				{
					Log.Warn($"Unknown event type found! \"{type}\"");
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error while dispatching event!", ex);
			}
		}*/

		private class EventDispatcherValues
		{
			//	private ConcurrentDictionary<IEventHandler, MethodInfo> EventHandlers { get; }
			private Dictionary<EventPriority, List<Item>> Items { get; }

			//private SortedSet<Item> Items { get; set; }
			public EventDispatcherValues()
			{
				Items = new Dictionary<EventPriority, List<Item>>();
				foreach (var prio in Enum.GetValues(typeof(EventPriority)))
				{
					Items.Add((EventPriority) prio, new System.Collections.Generic.List<Item>());
				}

				//Items = new SortedSet<Item>();
				//	EventHandlers = new ConcurrentDictionary<IEventHandler, MethodInfo>();
			}

			public bool RegisterEventHandler(EventHandlerAttribute attribute, object parent, MethodInfo method)
			{
				Items[attribute.Priority].Add(new Item(attribute, parent, method));
				return true;
				//return Items.Add(new Item(attribute, parent, method));
				/*if (!EventHandlers.TryAdd(parent, method))
				{
					return true;
				}
				return false;*/
			}

			public void Clear(object parent)
			{
				foreach (var priorityList in Items.ToArray())
				{
					try
					{
						var copy = priorityList.Value.ToArray();
						foreach (var item in copy)
						{
							try
							{
								if (item.Parent == parent)
								{
									if (priorityList.Value.Count > 0)
									{
										priorityList.Value.Remove(item);
									}
								}
							}
							catch (Exception x)
							{
							}
						}
					}
					catch (Exception ex)
					{
					}
				}

				//Items.RemoveWhere(x => x.Parent == parent);
				//MethodInfo method;
				//EventHandlers.TryRemove(parent, out method);
			}

			public void Dispatch(Event e)
			{
				object[] args =
				{
					e
				};

				foreach (var priority in Items.ToArray())
				{
					List<Item> toRemove = new List<Item>();

					Parallel.ForEach(priority.Value.ToArray(), pair =>
					{
						if (e.IsCancelled &&
							pair.Attribute.IgnoreCanceled)
							return;

						if (!pair.Parent.IsAlive)
						{
							toRemove.Add(pair);
							return;
						}

						pair.Method.Invoke(pair.Parent.Target, args);
					});

					foreach (var item in toRemove)
					{
						Items[priority.Key].Remove(item);
					}
				}
			}

			/*public async Task DispatchAsync(Event e)
			{
				object[] args = {
					e
				};

				foreach (var priority in Items)
				{
					Task[] tasks = new Task[priority.Value.Count];
					for (var index = 0; index < priority.Value.Count; index++)
					{
						var p = priority.Value[index];
						
						var method = p.Method;
						if (method.ReturnType == typeof(void))
						{
							tasks[index] = Task.Run(() =>
							{
								if (e.IsCancelled &&
								    p.Attribute.IgnoreCanceled)
									return;
								
								method.Invoke(p.Parent, args);
							});
						}
						else if (typeof(Task).IsAssignableFrom(method.ReturnType))
						{
							tasks[index] = (Task) method.Invoke(p.Parent, args);
						}
					}

					await Task.WhenAll(tasks);
				}
			}*/

			private struct Item : IComparable<Item>
			{
				//public EventPriority Priority;
				public EventHandlerAttribute Attribute;
				public WeakReference Parent;
				public MethodInfo            Method;

				internal Item(EventHandlerAttribute attribute, object parent, MethodInfo method)
				{
					Attribute = attribute;
					Parent    = new WeakReference(parent);
					Method    = method;
				}

				public int CompareTo(Item other)
				{
					int result = Attribute.Priority.CompareTo(other.Attribute.Priority);

					if (result == 0)
						result = Parent.Target.GetHashCode().CompareTo(other.Parent.Target.GetHashCode());

					return result;
				}
			}

			//private class ItemCompare
		}
	}

	/*public static class EventDispatcherExtensions
	{
		public static void RegisterEventHandlers<T>(this T eventHandler) where T : class
		{
			RegisterEventHandlers(eventHandler, EventDispatcher.Instance);
		}

		public static void RegisterEventHandlers<T>(this T eventHandler, EventDispatcher dispatcher) where T : class
		{
			dispatcher.RegisterEvents(eventHandler);
		}
	}*/
}