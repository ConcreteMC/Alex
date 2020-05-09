using System;
using System.Collections.Concurrent;
using System.Linq;
using Alex.API.Graphics;
using NLog;

namespace Alex.API.World
{
    public class TickManager
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(TickManager));
		
		private ConcurrentDictionary<Action, long> _scheduledTicks { get; }
		private long _tick = 0;
		public long CurrentTick => _tick;
	    public TickManager()
	    {
		    _scheduledTicks = new ConcurrentDictionary<Action, long>();
		}

		private double _lastTickTime = 0;
		public bool Update(IUpdateArgs args)
		{
			if ((args.GameTime.TotalGameTime.TotalMilliseconds - _lastTickTime) >= 50)
			{
				_lastTickTime = args.GameTime.TotalGameTime.TotalMilliseconds;
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
