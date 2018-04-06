using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Data;
using Alex.Gamestates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.Rendering.UI
{
	public class ChatComponent : UIComponent, IChatReceiver
	{
		public bool RenderChatInput { get; private set; } = false;
		private List<string> ChatMessages { get; set; } = new List<string>();
		private StringBuilder _input = new StringBuilder();

		public IChatProvider ChatProvider;
		public ChatComponent()
		{
			Alex.OnCharacterInput += OnCharacterInput;
		}

		public override void Render(RenderArgs args)
		{
			args.SpriteBatch.Begin();
			try
			{
				if (RenderChatInput)
				{
					var heightCalc = Alex.Font.MeasureString("!");
					string chatInput = _input.ToString().StripIllegalCharacters();
					if (chatInput.Length > 0)
					{
						heightCalc = Alex.Font.MeasureString(chatInput);
					}

					int extra = 0;
					if (heightCalc.X > args.GraphicsDevice.Viewport.Width / 2f)
					{
						extra = (int) (heightCalc.X - args.GraphicsDevice.Viewport.Width / 2f);
					}

					args.SpriteBatch.FillRectangle(
						new Rectangle(0, (int) (args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 5)),
							(args.GraphicsDevice.Viewport.Width / 2) + extra, (int) heightCalc.Y),
						new Color(Color.Black, 64));

					args.SpriteBatch.DrawString(Alex.Font, chatInput,
						new Vector2(5, (int) (args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 5))), Color.White);
				}

				if (ChatMessages.Count > 0)
				{
					var count = 2;
					foreach (var msg in ChatMessages.TakeLast(5).Reverse())
					{
						var amsg = msg.StripColors();
						amsg = amsg.StripIllegalCharacters();
						var heightCalc = Alex.Font.MeasureString(amsg);

						int extra = 0;
						if (heightCalc.X > args.GraphicsDevice.Viewport.Width / 2f)
						{
							extra = (int) (heightCalc.X - args.GraphicsDevice.Viewport.Width / 2f);
						}

						args.SpriteBatch.FillRectangle(
							new Rectangle(0, (int) (args.GraphicsDevice.Viewport.Height - ((heightCalc.Y * count) + 10)),
								(args.GraphicsDevice.Viewport.Width / 2) + extra, (int) heightCalc.Y),
							new Color(Color.Black, 64));
						args.SpriteBatch.DrawString(Alex.Font, amsg,
							new Vector2(5, (int) (args.GraphicsDevice.Viewport.Height - ((heightCalc.Y * count) + 10))), Color.White);
						count++;
					}
				}
			}
			finally
			{
				args.SpriteBatch.End();
			}
		}

		private KeyboardState _prevKeyboardState;
		public override void Update(GameTime time)
		{
			KeyboardState currentKeyboardState = Keyboard.GetState();
			if (currentKeyboardState != _prevKeyboardState)
			{
				if (RenderChatInput) //Handle Input
				{					
				//	if (currentKeyboardState.IsKeyDown(Keys.Back))
				//	{
				//		BackSpace();
				//	}

					if (currentKeyboardState.IsKeyDown(Keys.Enter))
					{
						SubmitMessage();
					}
				}
				else
				{
					if (currentKeyboardState.IsKeyDown(KeyBinds.Chat))
					{
						RenderChatInput = !RenderChatInput;
						_input.Clear();
					}
				}

				
			}
			_prevKeyboardState = currentKeyboardState;
		}

		public void Dismiss()
		{
			RenderChatInput = false;
			_input.Clear();
		}

		public void Clear()
		{

		}

		private void OnCharacterInput(object sender, TextInputEventArgs c)
		{
			if (RenderChatInput)
			{
				if (c.Key == Keys.Back)
				{
					BackSpace();
					return;
				}

				if (c.Key == Keys.Enter)
				{
					SubmitMessage();
					return;
				}

				_input.Append(c.Character);
			}
		}

		private DateTime _lastBackSpace = DateTime.MinValue;
		private void BackSpace()
		{
			if (DateTime.UtcNow.Subtract(_lastBackSpace).TotalMilliseconds < 100) return;
			_lastBackSpace = DateTime.UtcNow;
			if (_input.Length > 0) _input = _input.Remove(_input.Length - 1, 1);
		}

		private void SubmitMessage()
		{
			//Submit message
			if (_input.Length > 0)
			{
				if (Alex.IsMultiplayer)
				{
					ChatProvider?.Send(_input.ToString());
					//Client.SendChat(_input);
				}
				else
				{
					ChatMessages.Add("<Alex> " + _input.ToString());
				}
			}
			_input.Clear();
			RenderChatInput = false;
		}

		public void Append(string message)
		{
			ChatMessages.Add(message);
		}

		public void Receive(string message)
		{
			Append(message);
		}
	}
}
