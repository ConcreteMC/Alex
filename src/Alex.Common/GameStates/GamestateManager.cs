using System;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Common.GameStates
{
    public class GameStateManager : DrawableGameComponent
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GameStateManager));

		private ConcurrentDictionary<string, IGameState> States { get; }

		private IGameState ActiveState { get; set; }
		private GraphicsDevice Graphics { get; }
        private SpriteBatch SpriteBatch { get; }
        public GameStateManager(Game game, GraphicsDevice graphics, SpriteBatch spriteBatch) : base(game)
        { 
            Graphics = graphics;
            SpriteBatch = spriteBatch;

            States = new ConcurrentDictionary<string, IGameState>(StringComparer.OrdinalIgnoreCase);
        }

        public void Back()
        {
	        var currentState = ActiveState;

	        if (currentState.ParentState != null)
	        {
		        SetActiveState(currentState.ParentState, false);
	        }
        }

        public void AddState<TStateType>(string name) where TStateType : class, IGameState, new()
	    {
			AddState(name, new TStateType());
	    }

        public bool AddState(string name, IGameState state)
        {
	        if (States.TryAdd(name, state))
	        {
		        state.Identifier = name;
		        return true;
	        }

	        return false;
        }

        public bool TryGetState<TStateType>(string name, out TStateType state) where TStateType : class, IGameState
        {
	        if (States.TryGetValue(name, out IGameState s))
	        {
		        if (s is TStateType type)
		        {
			        state = type;
			        return true;
		        }
	        }

	        state = default;
	        return false;
        }

        public bool RemoveState<TStateType>(TStateType state) where TStateType : class, IGameState
        {
	        return RemoveState(state.Identifier);
        }
        
	    public bool RemoveState(string name)
	    {
		    IGameState state;
		    if (States.TryRemove(name, out state))
		    {
			    EnsureStateUnloaded(state);
			    return true;
		    }

		    return false;
	    }

	    private void EnsureStateUnloaded(IGameState state)
	    {
		    if (ActiveState == state)
		    {
			    SetActiveState((IGameState)null);
		    }
		    
		    state.Unload();
	    }
	    
		private void EnsureStateLoaded(IGameState state)
		{
			if (state == null)
				return;
			
			if (state.ParentState == state)
				state.ParentState = null;
			
			state.Load(new RenderArgs()
			{
				SpriteBatch    = SpriteBatch,
				GraphicsDevice = Graphics,
				GameTime       = new GameTime()
			});
		}

		public bool SetActiveState<TStateType>(string key, bool keepHistory) where TStateType : IGameState, new()
		{
			if (!States.TryGetValue(key, out var state))
			{
				state = new TStateType();
				AddState(key, state);
			}

			return SetActiveState(state, keepHistory);
		}
		
	    public bool SetActiveState<TStateType>(string key) where TStateType : IGameState, new()
	    {
		    return SetActiveState<TStateType>(key, true);
	    }

	    public bool SetActiveState<TStateType>() where TStateType : IGameState, new()
	    {
		    return SetActiveState<TStateType>(true);
	    }

	    public bool SetActiveState<TStateType>(bool keepHistory) where TStateType : IGameState, new()
	    {
		    var key = typeof(TStateType).FullName;
		    return SetActiveState<TStateType>(key, keepHistory);
	    }

	    public bool SetActiveState(IGameState state, bool keepHistory = true)
	    {
		    var currentState = ActiveState;

		    if (state != null && state.ParentState == null && keepHistory)
		    {
			    if (currentState != null && currentState.ParentState != state && state != currentState)
			    {
				    state.ParentState = currentState;
			    }
		    }

		    EnsureStateLoaded(state);
		    
		    if (currentState != null)
			    currentState.Hide();
		    
		    ActiveState = state;
		    state?.Show();

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

	    private void ClearStates()
	    {
		    var states = States.ToArray();
		    foreach (var gamestate in states)
		    {
			    RemoveState(gamestate.Key);
		    }
	    }

	    /// <inheritdoc />
	    protected override void UnloadContent()
	    {
		    ClearStates();
		    
		    base.UnloadContent();
	    }

	    /// <inheritdoc />
	    protected override void Dispose(bool disposing)
	    {
		    base.Dispose(disposing);

		    if (disposing)
		    {
			    var activeState = ActiveState;
			    ActiveState = null;
			    
			    if (activeState != null)
			    {
				    EnsureStateUnloaded(activeState);
			    }
		    }
	    }
    }
}
