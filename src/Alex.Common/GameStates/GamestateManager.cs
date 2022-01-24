using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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

		public void AddState<TStateType>(string name) where TStateType : class, IGameState
		{
			AddState(name, Construct<TStateType>());
		}

		public bool AddState(string name, IGameState state)
		{
			return States.TryAdd(name, state);
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
			var match = States.FirstOrDefault(x => x.Value == state).Key;

			if (match == null)
				return false;

			return RemoveState(match);
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

			state?.Unload();
		}

		private void EnsureStateLoaded(IGameState state)
		{
			if (state == null)
				return;

			if (state.ParentState == state)
				state.ParentState = null;

			state.Load(
				new RenderArgs() { SpriteBatch = SpriteBatch, GraphicsDevice = Graphics, GameTime = new GameTime() });
		}

		private TGameState Construct<TGameState>() where TGameState : class, IGameState
		{
			var serviceProvider = base.Game.Services.GetRequiredService<IServiceProvider>();

			TGameState state = null;

			foreach (var constructor in (typeof(TGameState).GetConstructors()))
			{
				bool canConstruct = true;
				object[] passedParameters = new object[0];
				var objparams = constructor.GetParameters();

				passedParameters = new object[objparams.Length];

				for (var index = 0; index < objparams.Length; index++)
				{
					var param = objparams[index];
					var p = serviceProvider.GetService(param.ParameterType);

					if (p != null)
					{
						passedParameters[index] = p;
					}
					else
					{
						canConstruct = false;

						break;
					}
				}

				if (canConstruct)
				{
					state = (TGameState)constructor.Invoke(passedParameters);

					break;
				}
			}

			return state;
		}

		public bool SetActiveState<TStateType>() where TStateType : class, IGameState
		{
			return SetActiveState<TStateType>(true);
		}

		public bool SetActiveState<TStateType>(bool keepHistory) where TStateType : class, IGameState
		{
			return SetActiveState<TStateType>(keepHistory, true);
		}

		public bool SetActiveState<TStateType>(bool keepHistory, bool keepLoaded) where TStateType : class, IGameState
		{
			var key = typeof(TStateType).FullName;

			return SetActiveState<TStateType>(key, keepHistory, keepLoaded);
		}

		public bool SetActiveState<TStateType>(string key, bool keepHistory) where TStateType : class, IGameState
		{
			return SetActiveState<TStateType>(key, keepHistory, true);
		}

		public bool SetActiveState<TStateType>(string key, bool keepHistory, bool keepLoaded)
			where TStateType : class, IGameState
		{
			if (!States.TryGetValue(key, out var state))
			{
				state = Construct<TStateType>();
			}

			return SetActiveState(state, keepHistory, keepLoaded);
		}

		public bool SetActiveState<TStateType>(string key) where TStateType : class, IGameState
		{
			return SetActiveState<TStateType>(key, true);
		}

		// public bool SetActiveState<TStateType>() where TStateType : IGameState, new()
		//  {
		//    return SetActiveState<TStateType>(true);
		// }

		public bool SetActiveState(IGameState state, string key, bool keepHistory, bool keepLoaded)
		{
			if (key != null)
			{
				bool isKnownState = States.TryGetValue(key, out _);

				if (keepLoaded && !isKnownState)
				{
					state.StateBehavior = StateBehavior.Referenced;
					AddState(key, state);
				}
				else if (!isKnownState)
				{
					state.StateBehavior = StateBehavior.Unload;
				}
			}

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
			{
				currentState.Hide();
			}

			ActiveState = state;
			state?.Show();

			if (currentState != null)
			{
				if (!keepHistory && currentState.StateBehavior == StateBehavior.Unload)
				{
					//  EnsureStateUnloaded(currentState);
				}
			}

			return true;
		}

		public bool SetActiveState(IGameState state, bool keepHistory = true)
		{
			return SetActiveState(state, keepHistory, false);
		}

		public bool SetActiveState(IGameState state, bool keepHistory, bool keepLoaded)
		{
			return SetActiveState(state, state?.GetType()?.FullName, keepHistory, keepLoaded);
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
					SpriteBatch = SpriteBatch, GameTime = gameTime, GraphicsDevice = Graphics
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