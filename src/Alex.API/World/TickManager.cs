using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Alex.API.Graphics;
using MiNET.Utils;
using NLog;

namespace Alex.API.World
{
    public class TickManager : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(TickManager));
		
		private object _tickLock = new object();
		private ConcurrentDictionary<Action, long> _scheduledTicks { get; }
		private LinkedList<ITicked>                _tickedItems    { get; }
		private long                               _tick = 0;
		public  long                               CurrentTick => _tick;

		private HighPrecisionTimer TickTimer      { get; set; }
		public  double             TicksPerSecond { get; set; }

		public TickManager()
	    {
		    _scheduledTicks = new ConcurrentDictionary<Action, long>();
		    _tickedItems = new LinkedList<ITicked>();
		    TickTimer = new HighPrecisionTimer(50, DoTick, false, false);
		}

		private Stopwatch _sw = Stopwatch.StartNew();
	    private void DoTick(object state)
	    {
		    lock (_tickLock)
		    {
			    var startTime = _sw.ElapsedMilliseconds;
			    var ticks     = _scheduledTicks.Where(x => x.Value <= _tick).ToArray();

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

			    var scheduledTicksTime = _sw.ElapsedMilliseconds - startTime;

			    var tickedItems = _tickedItems.ToArray();
			    foreach (var ticked in tickedItems)
			    {  
					try
				    {
				    	ticked.OnTick();
				    }
				    catch (Exception ex)
				    {
					    Log.Error(ex, $"An exception occureced while executing a scheduled tick!");
				    }
			    }

			    var endTime = _sw.ElapsedMilliseconds;

			    var elapsedTickTime = endTime - startTime;

			    if (elapsedTickTime > 50)
			    {
				//    Log.Warn($"Ticking running slow! Tick took: {elapsedTickTime}ms of which {scheduledTicksTime} were spent on scheduled ticks. (ScheduledTicks={ticks.Length} TickedItems={tickedItems.Length})");
			    }
			    
			    _tick++;

			    if (_tick % 20 == 0)
			    {
				    var elapsed = _sw.Elapsed;
				    _sw.Restart();

				    TicksPerSecond = 20d / elapsed.TotalSeconds;

				    if (elapsed.TotalMilliseconds <= 950)
				    {
					    Log.Warn($"Running ahead! TPS: {TicksPerSecond}");
				    }
				    else if (elapsed.TotalMilliseconds >= 1050)
				    {
					    Log.Warn($"Running behind! TPS: {TicksPerSecond}");
				    }
				    
				    if (Math.Ceiling(TicksPerSecond) < 20)
				    {
					 //   Log.Warn($"Running behind! TPS: {TicksPerSecond}");
				    }
			    }
		    }
	    }

	    public void RegisterTicked(ITicked ticked)
	    {
		    lock (_tickLock)
		    {
			    _tickedItems.AddLast(ticked);
		    }
	    }

	    public void UnregisterTicked(ITicked ticked)
	    {
		    lock (_tickLock)
		    {
			    _tickedItems.Remove(ticked);
		    }
	    }
	    
	    public void ScheduleTick(Action action, long ticksFromNow)
		{
			if (!_scheduledTicks.TryAdd(action, _tick + ticksFromNow))
			{
				Log.Warn($"Could not schedule tick!");
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_scheduledTicks?.Clear();
			_tickedItems?.Clear();
			
			try
			{
				TickTimer?.Dispose();
				TickTimer = null;
			}catch(ObjectDisposedException){}
		}
	}

    public interface ITicked
    {
	    void OnTick();
    }
}
