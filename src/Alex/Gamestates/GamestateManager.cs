using System;
using System.Collections.Concurrent;
using System.Threading;
using Alex.Gui;
using Alex.Gui.Themes;
using Alex.Rendering;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
    public class GameStateManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GameStateManager));
        
        private ConcurrentDictionary<string, GameState> ActiveStates { get; }
        private GameState ActiveState { get; set; }

        private GraphicsDevice Graphics { get; }
        private SpriteBatch SpriteBatch { get; }

	    private GuiManager GuiManager { get; }

		private ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        public GameStateManager(GraphicsDevice graphics, SpriteBatch spriteBatch, GuiManager guiManager)
        {
            Graphics = graphics;
            SpriteBatch = spriteBatch;
	        GuiManager = guiManager;

            ActiveStates = new ConcurrentDictionary<string, GameState>();
		}

        public void AddState(string name, GameState state)
        {
            state.Load(new RenderArgs()
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
            GameState state;
            if (ActiveStates.TryRemove(name, out state))
            {
				// lock (_lock)
				Lock.EnterUpgradeableReadLock();
	            try
	            {
		            if (ActiveState == state)
		            {
			            SetActiveState((GameState) null);
		            }
	            }
	            finally
	            {
					Lock.ExitUpgradeableReadLock();
	            }

	            state.Unload();
				return true;
            }
            return false;
        }

        public bool SetActiveState(GameState state)
        {
			Lock.EnterWriteLock();
	        try
	        {
				ActiveState?.Hide();
		        ActiveState = state;
				state?.Show();
	        }
	        finally
	        {
		        Lock.ExitWriteLock();
	        }

	        return true;
        }

        public bool SetActiveState(string name)
        {
            GameState state;
            if (!ActiveStates.TryGetValue(name, out state))
            {
                return false;
            }

	        return SetActiveState(state);
        }

        public void Draw(GameTime gameTime)
        {
	        GameState activeState;
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

	                activeState.Draw3D(args);
	                activeState.Draw2D(args);
                }
                catch (Exception ex)
                {
                    Log.Warn("An exception occured while trying to render!", ex);
                }
            }
        }

        public void Update(GameTime gameTime)
		{
			GameState activeState;
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
	                    activeState.Update(gameTime);
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
