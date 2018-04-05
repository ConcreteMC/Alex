using System;
using System.Collections.Concurrent;
using System.Linq;
using Alex.API.Graphics;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.API.World
{
    public class TickManager
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(TickManager));

		private IWorld World { get; }
		private ConcurrentDictionary<Action, long> _scheduledTicks { get; }
		private long _tick = 0;
	    public TickManager(IWorld world)
	    {
		    World = world;
		    _scheduledTicks = new ConcurrentDictionary<Action, long>();
		}

		private TimeSpan _lastTickTime = TimeSpan.Zero;
		public bool Update(IUpdateArgs args)
		{
			if ((args.GameTime.TotalGameTime - _lastTickTime).TotalMilliseconds >= 50)
			{
				_lastTickTime = args.GameTime.TotalGameTime;
				var ticks = _scheduledTicks.Where(x => x.Value <= _tick).ToArray();

				foreach (var tick in ticks)
				{
					_scheduledTicks.TryRemove(tick.Key, out long _);
				}

				//Executed scheduled ticks
				foreach (var tick in ticks)
				{
					try
					{
						tick.Key.Invoke();
					}
					catch (Exception ex)
					{
						Log.Error(ex, $"An exception occureced while executing a scheduled tick!");
					}
				}

				_tick++;
				return true;
			}

			return false;
		}

		public void ScheduleTick(Action action, long ticksFromNow)
		{
			if (!_scheduledTicks.TryAdd(action, _tick + ticksFromNow))
			{
				Log.Warn($"Could not schedule tick!");
			}
		}
    }
}
