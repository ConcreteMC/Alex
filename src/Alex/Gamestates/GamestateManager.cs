using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.Common.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using RocketUI;

namespace Alex.Gamestates
{
    public class GameStateManager : DrawableGameComponent
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GameStateManager));

		private ConcurrentDictionary<string, IGameState> States { get; }

		private LinkedList<IGameState> History { get; } = new LinkedList<IGameState>();
		private readonly object _historyLock = new object();
		
        private IGameState ActiveState { get; set; }

        private GraphicsDevice Graphics { get; }
        private SpriteBatch SpriteBatch { get; }
        
        public GameStateManager(Game game, GraphicsDevice graphics, SpriteBatch spriteBatch) : base(game)
        { 
            Graphics = graphics;
            SpriteBatch = spriteBatch;

            States = new ConcurrentDictionary<string, IGameState>();
		}

        public void Back()
	    {
		    lock (_historyLock)
		    {
			    var last = History.Last;
			    if (last != null)
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

        public bool RemoveState<TStateType>(TStateType state) where TStateType : class, IGameState
        {
	        var key = States.FirstOrDefault(x => x.Value == state);

	        if (key.Key == null)
		        return false;

	        return RemoveState(key.Key);
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


	    public bool SetActiveState(IGameState state, bool keepHistory = true)
	    {
			EnsureStateLoaded(state);

		    var previous = ActiveState;
		    if (keepHistory && previous != null && state != null && state.ParentState == null)
		    {
			    state.ParentState = previous;
		    }

		    ActiveState = state;
		    ActiveState?.Show();
		    previous?.Hide();

		    if (keepHistory)
		    {
			    lock (_historyLock)
			    {
				    if (History.Last?.Previous?.Value != state)
				    {
					    History.AddLast(previous);
				    }
			    }
		    }
		    
		    return true;
	    }

	    public bool SetActiveState(string name, bool keepHistory = true)
        {
	        IGameState state;
            if (!States.TryGetValue(name, out state))
            {
                return false;
            }

	        return SetActiveState(state, keepHistory);
        }

	   // private IGameState _activeStateDoubleBuffer = null;

	    
	    public override void Draw(GameTime gameTime)
	    {
		    IGameState activeState = ActiveState;

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

	    public override void Update(GameTime gameTime)
	    {
		    IGameState activeState = ActiveState;

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

	    private void ClearStates()
	    {
		    var states = States.ToArray();
		    
		    States.Clear();
		    
		    foreach (var gamestate in states)
		    {
			    gamestate.Value?.Unload();
		    }

		    var activeState = ActiveState;

		    if (activeState != null)
		    {
			    activeState.Hide();
			    activeState.Unload();
			    
			    ActiveState = null;
		    }
	    }

	    /// <inheritdoc />
	    protected override void UnloadContent()
	    {
		    Log.Info($"Unload content...");
		    ClearStates();
		    
		    base.UnloadContent();
	    }
    }
}
