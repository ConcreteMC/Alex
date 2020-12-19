using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Alex.API.GameStates;
using Alex.API.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Gamestates
{
    public class GameStateManager
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GameStateManager));

		private ConcurrentDictionary<string, IGameState> States { get; }

		private LinkedList<IGameState> History { get; } = new LinkedList<IGameState>();
		private object _historyLock { get; } = new object();
		
        private IGameState ActiveState { get; set; }

        private GraphicsDevice Graphics { get; }
        private SpriteBatch SpriteBatch { get; }
		
	    public GuiManager GuiManager { get; private set; }
        public GameStateManager(GraphicsDevice graphics, SpriteBatch spriteBatch, GuiManager guiManager)
        { 
            Graphics = graphics;
            SpriteBatch = spriteBatch;
	        GuiManager = guiManager;

            States = new ConcurrentDictionary<string, IGameState>();
		}

	    public void Back()
	    {
		    lock (_historyLock)
		    {
			    var last = History.Last;
			    if (History.Last != null)
			    {
				    var prev = last.Value;
				    if (prev != ActiveState)
				    {
					    History.RemoveLast();
					    SetActiveState(prev, false);
				    }
			    }
		    }
	    }

	    public void AddState<TStateType>(string name) where TStateType : class, IGameState, new()
	    {
			AddState(name, new TStateType());
	    }

        public void AddState(string name, IGameState state)
        {
			EnsureStateLoaded(state);

            States.AddOrUpdate(name, state, (s, gamestate) =>
            {
                return state;
            });
        }

        public bool TryGetState<TStateType>(string name, out TStateType state) where TStateType : class, IGameState
        {
	        if (States.TryGetValue(name, out IGameState s))
	        {
		        if (s is TStateType)
		        {
			        state = (TStateType) s;
			        return true;
		        }
	        }

	        state = default;
	        return false;
        }

	    public bool RemoveState(string name)
	    {
		    IGameState state;
		    if (States.TryRemove(name, out state))
		    {
			    if (ActiveState == state)
			    {
				    var parent = state.ParentState;
				    if (parent == null)
				    {
					    SetActiveState((IGameState) null);
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

		private void EnsureStateLoaded(IGameState state)
		{
			//if(state.IsLoaded) return;
			state.Load(new RenderArgs()
			{
				SpriteBatch    = SpriteBatch,
				GraphicsDevice = Graphics,
				GameTime       = new GameTime()
			});
		}

	    public bool SetActiveState<TStateType>(string key) where TStateType : IGameState, new()
	    {
		    if (!States.TryGetValue(key, out var state))
		    {
			    state = new TStateType();
			    AddState(key, state);
		    }

		    return SetActiveState(state);
	    }

	    public bool SetActiveState<TStateType>() where TStateType : IGameState, new()
	    {
		    var key = typeof(TStateType).FullName;
		    return SetActiveState<TStateType>(key);
	    }

	    public bool SetAndUpdateActiveState<TStateType>(Func<TStateType, TStateType> doActionBeforeSwitching) where TStateType : IGameState, new()
	    {
		    IGameState state = null;
			var key = typeof(TStateType).FullName;
		    if (States.TryGetValue(key, out state))
		    {
			    state = doActionBeforeSwitching.Invoke((TStateType)state);
			    return SetActiveState(state);
		    }

		    state = new TStateType();
		    state = doActionBeforeSwitching.Invoke((TStateType)state);
			AddState(key, state);

			return SetActiveState(state);
	    }


		public bool SetActiveState(IGameState state, bool keepHistory = true)
	    {
			EnsureStateLoaded(state);

		    var current = ActiveState;
		    current?.Hide();

		    if (current != null && state != null && state.ParentState == null)
		    {
			    state.ParentState = current;
		    }

		    ActiveState = state;
		    ActiveState?.Show();

		    lock (_historyLock)
		    {
			    if (History.Last?.Previous?.Value != state && keepHistory)
			    {
				    History.AddLast(current);
			    }
		    }

		    _activeStateDoubleBuffer = state;

		    return true;
	    }

	    public bool SetActiveState(string name)
        {
	        IGameState state;
            if (!States.TryGetValue(name, out state))
            {
                return false;
            }

	        return SetActiveState(state);
        }

	    private IGameState _activeStateDoubleBuffer = null;

	    public void Draw(GameTime gameTime)
	    {
		    IGameState activeState = _activeStateDoubleBuffer;

		    if (activeState == null) return;

		    try
		    {
			    RenderArgs args = new RenderArgs()
			    {
				    SpriteBatch = SpriteBatch,
				    GameTime = gameTime,
				    GraphicsDevice = Graphics
			    };

			    activeState.Draw(args);
		    }
		    catch (Exception ex)
		    {
			    Log.Warn(ex, $"An exception occured while trying to render: {ex.ToString()}");
		    }
	    }

	    public void Update(GameTime gameTime)
	    {
		    IGameState activeState = _activeStateDoubleBuffer;

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

	    public IGameState GetActiveState()
	    {
		    return ActiveState;
	    }

	    public IGameState GetPreviousState()
	    {
		    lock (_historyLock)
		    {
			    return History.Last.Value;
		    }
	    }
	}
}
