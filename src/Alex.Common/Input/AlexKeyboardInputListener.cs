using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace Alex.Common.Input
{
	public class AlexKeyboardInputListener : InputListenerBase<AlexKeyboardInputListener.AlexKeyState, Keys[]>
	{
		public static EventHandler<AlexKeyboardInputListener> InstanceCreated;

		public AlexKeyboardInputListener() : this(PlayerIndex.One) { }

		public AlexKeyboardInputListener(PlayerIndex playerIndex) : base(playerIndex)
		{
			InstanceCreated?.Invoke(this, this);
		}

		protected override AlexKeyState GetCurrentState()
		{
			return AlexKeyState.GetState();
		}

		protected override bool IsButtonDown(AlexKeyState state, Keys[] buttons)
		{
			return state.IsKeyDown(buttons);
		}

		protected override bool IsButtonUp(AlexKeyState state, Keys[] buttons)
		{
			return state.IsKeyUp(buttons);
		}

		public void RegisterMultiMap(InputCommand command, params Keys[] keys)
		{
			RegisterMap(command, keys);
		}

		public class AlexKeyState
		{
			private KeyboardState _currentState;
			private Keys[] _pressedKeys;

			public AlexKeyState()
			{
				_currentState = Keyboard.GetState();
				_pressedKeys = _currentState.GetPressedKeys();
			}

			public bool IsKeyDown(params Keys[] key)
			{
				return
					key.All(x => _pressedKeys.Contains(x)); // _pressedKeys.Contains(key); _currentState.IsKeyDown(key);
			}

			public bool IsKeyUp(params Keys[] key)
			{
				return !key.All(x => _pressedKeys.Contains(x));
			}

			public static AlexKeyState GetState()
			{
				return new AlexKeyState();
			}
		}
	}
}