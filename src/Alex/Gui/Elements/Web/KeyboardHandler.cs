using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Alex.Gui.Elements.Web
{
	public enum KeyEventType
	{
		KeyUp = 1,
		KeyDown = 2,
		Char = 3
	}
	public class KeyEvent
	{
		public KeyEventType Type { get; set; }
		public int NativeKeyCode { get; set; }
		public int WindowsKeyCode { get; set; }
		public Keys Key { get; set; }
	}
	class KeyboardHandler
	{
		private KeyboardState ks, ks_old = Keyboard.GetState();
		private List<KeyEvent> Queue = new List<KeyEvent>();
		public void Update()
		{
			ks = Keyboard.GetState();

			foreach (Keys key in Enum.GetValues(typeof(Keys)))
			{
				if (!ks.IsKeyDown(key) && ks_old.IsKeyDown(key))
				{
					Queue.Add(new KeyEvent() { Type = KeyEventType.KeyUp, NativeKeyCode = (int)key, WindowsKeyCode = (int)key, Key = key });
					Queue.Add(new KeyEvent() { Type = KeyEventType.Char, NativeKeyCode = (int)key, WindowsKeyCode = (int)key, Key = key});
				}
				else if (ks.IsKeyDown(key) && !ks_old.IsKeyDown(key))
					Queue.Add(new KeyEvent() { Type = KeyEventType.KeyDown, NativeKeyCode = (int)key, WindowsKeyCode = (int)key, Key = key });
			}
			ks_old = ks;
		}

		public IEnumerable<KeyEvent> Query()
		{
			var ls = Queue;
			Queue = new List<KeyEvent>();
			return ls;
		}
	}
}