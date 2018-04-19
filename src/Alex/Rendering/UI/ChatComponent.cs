using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Data;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Rendering;
using Alex.API.Input;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NLog;

namespace Alex.Rendering.UI
{
	public class ChatComponent : GuiControl, IChatReceiver
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChatComponent));

		private SortedList<DateTime, ChatObject> _chatEntries = new SortedList<DateTime, ChatObject>(new DescendedDateComparer());
		public IChatProvider ChatProvider;

		private TextInputBuilder _textBuilder;
		public ChatComponent()
		{
			_textBuilder = new TextInputBuilder();
			Anchor = Alignment.BottomLeft;

			Height = 180;
			Width = 320;
		}

		private IFont Font;
		protected override void OnInit(IGuiRenderer renderer)
		{
			Font = renderer.Font;

			FocusOutlineThickness = Thickness.Zero;
			HighlightOutlineThickness = Thickness.Zero;
		}

		private Vector2 Scale { get; set; } = Vector2.One;

		private int _cursorPositionX;
		private float _cursorAlpha;

		protected override void OnUpdate(GameTime gameTime)
		{
			if (IsFocused)
			{
				var msg = _textBuilder.Text;
				var preCursor = msg.Substring(0, _textBuilder.CursorPosition);
				var cursorOffsetX = (int)Font.MeasureString(preCursor, Scale).X;
				_cursorPositionX = cursorOffsetX;

				var delta = (float)gameTime.TotalGameTime.TotalMilliseconds / 2;
				_cursorAlpha = (float)MathHelpers.SinInterpolation(0.1f, 0.5f, delta) * 2;
			}
			base.OnUpdate(gameTime);
		}

		private TimeSpan _renderTimeout = TimeSpan.FromSeconds(30);
		protected override void OnDraw(GuiRenderArgs args)
		{
			base.OnDraw(args);

			if (IsFocused)
			{
				var renderPos = (Bounds.BottomLeft() - new Vector2(0, 8)).ToPoint();

				string msg = _textBuilder.Text;

				args.FillRectangle(new Rectangle(renderPos.X, renderPos.Y - 2, Width, 10), new Color(Color.Black, 0.5f));

				args.DrawString(Font, msg, new Vector2(renderPos.X, renderPos.Y), TextColor.White, false, 0f, Vector2.Zero, opacity: 1f);

				if (args.GameTime.TotalGameTime.Seconds % 2 == 0)
				{
					var offsetX = renderPos.X + _cursorPositionX + 1;

					args.DrawLine(offsetX, (renderPos.Y), offsetX, (renderPos.Y + 8), Color.White * _cursorAlpha);
				}
			}

			if (_chatEntries.Count > 0)
			{
				DateTime now = DateTime.UtcNow;
				Vector2 offset = new Vector2(0, -33);
				foreach (var msg in _chatEntries.ToArray())
				{
					var elapse = now - msg.Key;
					float alpha = 1f;
					if (!IsFocused)
					{
						if (elapse > _renderTimeout)
						{
							continue;
						}

						alpha = (float) (1f - ((elapse.TotalMilliseconds / _renderTimeout.TotalMilliseconds) * 1f));
					}

					string message = msg.Value.RawMessage;
					foreach (var line in CalculateLines(message))
					{
						DrawChatLine(args, line, alpha, ref offset);
					}
				}
			}
		}

		private string GetFitting(string text, out string rest)
		{
			rest = string.Empty;

			var size = Font.MeasureString(text);
			while (size.X > Bounds.Width)
			{
				string current = text;
				text = current.Remove(current.Length - 1, 1);
				rest = current.Substring(current.Length - 1, 1) + rest;

				size = Font.MeasureString(text);
			}

			return text;
		}

		private string[] CalculateLines(string text)
		{
			var size = Font.MeasureString(text);
			if (size.X > Bounds.Width)
			{
				List<string> output = new List<string>();
				do
				{
					string result = GetFitting(text, out text);
					if (result.Length == 0) break;
					output.Add(result);
					
				} while (text.Length > 0);

				output.Reverse();

				return output.ToArray();
			}
			else
			{
				return new[] {text};
			}
		}

		private void DrawChatLine(GuiRenderArgs args, string text, float alpha, ref Vector2 offset)
		{
			var size = Font.MeasureString(text);

			var renderPos = Bounds.BottomLeft() + offset;

			args.FillRectangle(new Rectangle(renderPos.ToPoint(), new Point(Width, (int)Math.Ceiling(size.Y + 2))), new Color(Color.Black, alpha * 0.5f));

			Font.DrawString(args.SpriteBatch, text, renderPos + new Vector2(0, 2), TextColor.White, opacity: alpha);
			offset.Y -= (size.Y + 2);
		}

		protected override void OnKeyInput(char character, Keys key)
		{
			if (IsFocused)
			{
				if (key == Keys.Back)
				{
					_textBuilder.RemoveCharacter();
				}
				else if (key == Keys.Left)
				{
					_textBuilder.CursorPosition--;
				}
				else if (key == Keys.Right)
				{
					_textBuilder.CursorPosition++;
				}
				else if (key == Keys.Enter)
				{
					SubmitMessage();
				}
				else
				{
					_textBuilder.AppendCharacter(character);
				}
			}
		}

		public void Dismiss()
		{
			IsFocused = false;
			_textBuilder.Clear();
		}

		private void SubmitMessage()
		{
			//Submit message
			if (_textBuilder.Length > 0)
			{
				if (Alex.IsMultiplayer)
				{
					ChatProvider?.Send(_textBuilder.Text);
				}
				else
				{
					Receive(new ChatObject(_textBuilder.Text));
				}
			}
			_textBuilder.Clear();
			IsFocused = false;
		}

		public void Receive(ChatObject message)
		{
			_chatEntries.Add(DateTime.UtcNow, message);
		}

		private class DescendedDateComparer : IComparer<DateTime>
		{
			public int Compare(DateTime x, DateTime y)
			{
				// use the default comparer to do the original comparison for datetimes
				int ascendingResult = Comparer<DateTime>.Default.Compare(x, y);

				// turn the result around
				return 0 - ascendingResult;
			}
		}
	}
}
