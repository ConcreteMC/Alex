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
		private ConcurrentDictionary<TickedItem, long> _scheduledTicks { get; }
		private LinkedList<TickedEntry>                _tickedItems    { get; }
		private long                               _tick = 0;
		public  long                               CurrentTick => _tick;

		private HighPrecisionTimer TickTimer      { get; set; }
		public  double             TicksPerSecond { get; set; }

		public TickManager()
	    {
		    _scheduledTicks = new ConcurrentDictionary<TickedItem, long>();
		    _tickedItems = new LinkedList<TickedEntry>();
		    TickTimer = new HighPrecisionTimer(50, DoTick);
		}

		private Stopwatch _sw = Stopwatch.StartNew();
	    private void DoTick(object state)
	    {
		   // if (!Monitor.TryEnter(_tickLock, 0))
			//    return;

		    try
		    {
			    var startTime = _sw.ElapsedMilliseconds;
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

			    var scheduledTicksTime = _sw.ElapsedMilliseconds - startTime;

			    TickedEntry[] tickedItems;
			    lock (_tickLock)
			    {
				    tickedItems = _tickedItems.ToArray();
			    }

			    foreach (var ticked in tickedItems)
			    {
				    try
				    {
					    if (!ticked.Run())
					    {
						    Log.Warn($"Failed to tick item.");

						    lock (_tickLock)
						    {
							    _tickedItems.Remove(ticked);
						    }
					    }
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
		    finally
		    {
			    //Monitor.Exit(_tickLock);
		    }
	    }

	    public void RegisterTicked(ITicked ticked)
	    {
		    lock (_tickLock)
		    {
			    _tickedItems.AddLast(new TickedEntry(ticked));
		    }
	    }

	    public void UnregisterTicked(ITicked ticked)
	    {
		    lock (_tickLock)
		    {
			    var item = _tickedItems.FirstOrDefault(x => x.Equals(ticked));
			    if (item != null)
					_tickedItems.Remove(item);
		    }
	    }
	    
	    public void ScheduleTick(Action action, long ticksFromNow, CancellationToken cancellationToken)
		{
			if (!_scheduledTicks.TryAdd(new TickedItem(action, cancellationToken), _tick + ticksFromNow))
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

		private class TickedItem
		{
			private Action _action;
			private CancellationToken _cancellationToken;
			public TickedItem(Action action, CancellationToken cancellationToken)
			{
				_action = action;
				_cancellationToken = cancellationToken;
			}

			public void Invoke()
			{
				if (!_cancellationToken.IsCancellationRequested)
					_action?.Invoke();
			}
		}
		
		private class TickedEntry
		{
			private WeakReference<ITicked> _ticked;

			private Stopwatch _stopwatch = Stopwatch.StartNew();
			public TickedEntry(ITicked ticked) {
				_ticked = new WeakReference<ITicked>(ticked);
			}

			public TimeSpan ProcessingTime { get; private set; } = TimeSpan.Zero;
			public bool Run()
			{
				_stopwatch.Restart();
				try
				{
					if (_ticked.TryGetTarget(out var target))
					{
						target?.OnTick();

						return true;
					}

					return false;
				}
				finally
				{
					ProcessingTime = _stopwatch.Elapsed;
				}
			}

			public bool Equals(ITicked ticked)
			{
				if (_ticked.TryGetTarget(out var target))
				{
					return target == ticked;
				}

				return false;
			}
		}
	}

    public interface ITicked
    {
	    void OnTick();
    }
}
