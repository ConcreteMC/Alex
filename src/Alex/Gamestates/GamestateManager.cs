using System;
using System.Collections.Concurrent;
using Alex.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Gamestates
{
    public class GameStateManager
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GameStateManager));

		private ConcurrentDictionary<string, GameState> ActiveStates { get; }
        private GameState ActiveState { get; set; }
	    private GameState PreviousState { get; set; } = null;

        private GraphicsDevice Graphics { get; }
        private SpriteBatch SpriteBatch { get; }

	    private UiManager UiManager { get; }
        public GameStateManager(GraphicsDevice graphics, SpriteBatch spriteBatch, UiManager uiManager)
        {
            Graphics = graphics;
            SpriteBatch = spriteBatch;
	        UiManager = uiManager;

            ActiveStates = new ConcurrentDictionary<string, GameState>();
		}

	    public void Back()
	    {
		    var prev = PreviousState;

			if (prev != null)
		    {
			    SetActiveState(prev);
		    }
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
			    if (ActiveState == state)
			    {
				    var parent = state.ParentState;
				    if (parent == null)
				    {
					    SetActiveState((GameState) null);
					}
				    else
				    {
					  //  SetActiveState(state.ParentState);
				    }
			    }

			    state.Unload();
			    return true;
		    }

		    return false;
	    }

	    public bool SetActiveState(GameState state)
	    {
		    var current = ActiveState;
		    current?.Hide();

		    if (current != null && state.ParentState == null)
		    {
			    state.ParentState = current;
		    }

		    ActiveState = state;
		    ActiveState?.Show();

		    PreviousState = current;

		    _activeStateDoubleBuffer = state;

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

	    private GameState _activeStateDoubleBuffer = null;

	    public void Draw(GameTime gameTime)
	    {
		    GameState activeState = _activeStateDoubleBuffer;

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
			    Log.Warn(ex, "An exception occured while trying to render!");
		    }
	    }

	    public void Update(GameTime gameTime)
	    {
		    GameState activeState = _activeStateDoubleBuffer;

		    if (activeState == null) return;

		    try
		    {
			 //   var parent = activeState.ParentState;

			//	if (parent != null)
			//    {
			//		parent.Update(gameTime);
			 //   }

			    activeState.Update(gameTime);
		    }
		    catch (Exception ex)
		    {
			    Log.Warn(ex, $"An exception occured while trying to call Update: {ex.ToString()}!");
		    }
	    }
    }
}
