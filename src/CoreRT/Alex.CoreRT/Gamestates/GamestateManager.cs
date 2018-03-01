using System;
using System.Collections.Concurrent;
using System.Threading;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.CoreRT.Gamestates
{
    public class GamestateManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GamestateManager));
        
        private ConcurrentDictionary<string, Gamestate> ActiveStates { get; }
        private Gamestate ActiveState { get; set; }

        private GraphicsDevice Graphics { get; }
        private SpriteBatch SpriteBatch { get; }

		private ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        public GamestateManager(GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            Graphics = graphics;
            SpriteBatch = spriteBatch;

            ActiveStates = new ConcurrentDictionary<string, Gamestate>();
        }

        public void AddState(string name, Gamestate state)
        {
            state.Init(new RenderArgs()
            {
                SpriteBatch = SpriteBatch,
                GraphicsDevice = Graphics,
                GameTime = new GameTime()
            });

            ActiveStates.AddOrUpdate(name, state, (s, gamestate) =>
            {
                return state;
            });
        }

        public bool RemoveState(string name)
        {
            Gamestate state;
            if (ActiveStates.TryRemove(name, out state))
            {
				// lock (_lock)
				Lock.EnterUpgradeableReadLock();
	            try
	            {
		            if (ActiveState == state)
		            {
						Lock.EnterWriteLock();
			            try
			            {
				            ActiveState = null;
			            }
			            finally
			            {
							Lock.ExitWriteLock();
			            }
		            }
	            }
	            finally
	            {
					Lock.ExitUpgradeableReadLock();
	            }

	            state.Stop();
				return true;
            }
            return false;
        }

        public bool SetActiveState(Gamestate state)
        {
			Lock.EnterWriteLock();
	        try
	        {
		        ActiveState = state;
	        }
	        finally
	        {
		        Lock.ExitWriteLock();
	        }

	        return true;
        }

        public bool SetActiveState(string name)
        {
            Gamestate state;
            if (!ActiveStates.TryGetValue(name, out state))
            {
                return false;
            }

            Lock.EnterWriteLock();
	        try
	        {
		        ActiveState = state;
	        }
	        finally
	        {
				Lock.ExitWriteLock();
	        }
            return true;
        }

        public void Draw(GameTime gameTime)
        {
	        Gamestate activeState;
			Lock.EnterReadLock();
	        try
	        {
		        activeState = ActiveState;
	        }
	        finally
	        {
				Lock.ExitReadLock();
	        }

          //  lock (_lock)
            {
                if (activeState == null) return;

                try
                {
                    RenderArgs args = new RenderArgs()
                    {
                        SpriteBatch = SpriteBatch,
                        GameTime = gameTime,
                        GraphicsDevice = Graphics
                    };

	                activeState.Rendering3D(args);
	                activeState.Rendering2D(args);
                }
                catch (Exception ex)
                {
                    Log.Warn("An exception occured while trying to render!", ex);
                }
            }
        }

        public void Update(GameTime gameTime)
		{
			Gamestate activeState;
			Lock.EnterReadLock();
			try
			{
				activeState = ActiveState;
			}
			finally
			{
				Lock.ExitReadLock();
			}

			// foreach (var i in ActiveStates.ToArray())
			{
                try
                {
                   // lock (_lock)
                    {
	                    activeState.UpdateCall(gameTime);
                    }
                   // i.Value.UpdateCall(gameTime);
                }
                catch(Exception ex)
                {
                    Log.Warn($"An exception occured while trying to call Update!", ex);
                }
            }
        }
    }
}
