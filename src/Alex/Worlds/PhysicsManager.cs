using System;
using System.Linq;
using System.Threading;
using Alex.API;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Utils;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Worlds
{
    public class PhysicsManager : IDisposable
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PhysicsManager));

	    private Alex Alex { get; }
		private IWorld World { get; }

		private System.Threading.Timer Timer = null;// = new System.Threading.Timer(GameTick, null, 50, 50);
		private object _timerLock = new object();
	    public PhysicsManager(Alex alex, IWorld world)
	    {
		    Alex = alex;
		    World = world;
	    }

	    public void Start()
	    {
		    if (Timer == null)
		    {
			    Timer = new System.Threading.Timer(GameTick, null, 50, 50);
		    }
		    else
		    {
			    Timer.Change(50, 50);
		    }
	    }

		private ThreadSafeList<IPhysicsEntity> PhysicsEntities { get; } = new ThreadSafeList<IPhysicsEntity>();
 	    private long SkippedTicks = 0;
	    private void GameTick(object state)
	    {
		    if (!Monitor.TryEnter(_timerLock))
		    {
			    SkippedTicks++;
				Log.Warn($"Skipped {SkippedTicks} ticks, something is taking to long!");
				return;
		    }

		    SkippedTicks = 0;

			try
		    {
			    foreach (var tickable in PhysicsEntities.ToArray())
			    {
				    try
				    {
					   tickable.OnTick();
					}
				    catch (Exception ex)
				    {
						Log.Warn(ex, $"Entity tick threw exception: {ex.ToString()}");
				    }
			    }
		    }
		    finally
		    {
				Monitor.Exit(_timerLock);
		    }
	    }

	    public void Stop()
	    {
		    Timer.Change(Timeout.Infinite, Timeout.Infinite);
	    }

	    public void Dispose()
	    {
		    Timer?.Dispose();
	    }

	    public bool AddTickable(IPhysicsEntity entity)
	    {
		    return PhysicsEntities.TryAdd(entity);
	    }

	    public bool Remove(IPhysicsEntity entity)
	    {
		    return PhysicsEntities.Remove(entity);
	    }
    }
}
