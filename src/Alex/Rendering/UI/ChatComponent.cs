using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Data;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.Gamestates;
using Alex.Gamestates.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;

namespace Alex.Rendering.UI
{
	public class ChatComponent : GuiContainer, IChatReceiver
	{
		private class ChatEntry
		{
			public ChatObject ChatObject;
			public DateTime ReceiveTime;
		}

		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChatComponent));

		public bool RenderChatInput { get; private set; } = false;
		private StringBuilder _input = new StringBuilder();

		private SortedList<DateTime, ChatObject> _chatEntries = new SortedList<DateTime, ChatObject>();
		public IChatProvider ChatProvider;
		public ChatComponent()
		{
			Anchor = Alignment.BottomLeft;

			BackgroundOverlayColor = new Color(Color.Black, 0.5f);
		}

		private BitmapFont Font;
		protected override void OnInit(IGuiRenderer renderer)
		{
			Font = renderer.Font;
			renderer.ScaledResolution.ScaleChanged += ScaledResolutionOnScaleChanged;
		}

		private void ScaledResolutionOnScaleChanged(object sender, UiScaleEventArgs e)
		{
				
		}

		protected override void OnDraw(GuiRenderArgs args)
		{
			//args.BeginSpriteBatch();
			if (_chatEntries.Count > 0)
			{
				DateTime now = DateTime.UtcNow;
				Vector2 offset = Vector2.One;
				foreach (var msg in _chatEntries.ToArray())
				{
					offset.Y += Font.MeasureString(msg.Value.RawMessage).Y;
					
				}
			}
		//	args.EndSpriteBatch();
			base.OnDraw(args);
		}

		protected override void OnUpdateLayout()
		{
			base.OnUpdateLayout();
		}

		/*public void Render(RenderArgs args)
		{
			Vector2 scale = new Vector2(1.25f, 1.25f);

			float horizontalOffset = 5;
			var heightCalc = Alex.Font.GetStringSize("!", scale);

			args.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			try
			{
				if (RenderChatInput)
				{
					string chatInput = _input.ToString();
					if (chatInput.Length > 0)
					{
						heightCalc = Alex.FontRender.GetStringSize(chatInput, scale);
					}

					int extra = 0;
					if (heightCalc.X > args.GraphicsDevice.Viewport.Width / 2f)
					{
						extra = (int) (heightCalc.X - args.GraphicsDevice.Viewport.Width / 2f);
					}

					args.SpriteBatch.FillRectangle(
						new Rectangle(0, (int) (args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 25)),
							(args.GraphicsDevice.Viewport.Width / 2) + extra, (int) heightCalc.Y),
						new Color(Color.Black, 64));

					Alex.FontRender.DrawString(args.SpriteBatch, chatInput, new Vector2(5, (int)(args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 25))), Color.White, false, scale);
				}

				if (ChatMessages.Count > 0)
				{
					var count = 2;
					foreach (var msg in ChatMessages.TakeLast(5).Reverse())
					{
						heightCalc = Alex.FontRender.GetStringSize(msg.RawMessage, scale);

						int extra = 0;
						if (heightCalc.X > args.GraphicsDevice.Viewport.Width / 2f)
						{
							extra = (int) (heightCalc.X - args.GraphicsDevice.Viewport.Width / 2f);
						}

						args.SpriteBatch.FillRectangle(
							new Rectangle(0, (int) (args.GraphicsDevice.Viewport.Height - ((heightCalc.Y * count) + 25)),
								(args.GraphicsDevice.Viewport.Width / 2) + extra, (int) heightCalc.Y),
							new Color(Color.Black, 64));

						var p = new Vector2(horizontalOffset,
							(int) (args.GraphicsDevice.Viewport.Height - ((heightCalc.Y * count) + 25)));
						Alex.FontRender.DrawString(args.SpriteBatch, msg.RawMessage, p, Color.White, false, scale);

						count++;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Could not render text: {ex.ToString()}");
			}
			finally
			{
				args.SpriteBatch.End();
			}
		}*/

		/*	private KeyboardState _prevKeyboardState;
			public void Update(GameTime time)
			{
				KeyboardState currentKeyboardState = Keyboard.GetState();
				if (currentKeyboardState != _prevKeyboardState)
				{
					if (RenderChatInput) //Handle Input
					{					
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
			}*/

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
				}
				else
				{
					Receive(new ChatObject(_input.ToString()));
				}
			}
			_input.Clear();
			RenderChatInput = false;
		}

		public void Receive(ChatObject message)
		{
			_chatEntries.Add(DateTime.UtcNow, message);
		}
	}
}
