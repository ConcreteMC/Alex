using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Data;
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MiNET;
using NLog;
using RocketUI;

namespace Alex.Gui.Elements
{
	public class ChatComponent : GuiTextInput
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChatComponent));
		
		private ConcurrentDeck<(string message, DateTime time)> _chatEntries = new ConcurrentDeck<(string message, DateTime time)>(10);

		public int UnfocusedHeight { get; set; } = 100;
		public int FocusedHeight { get; set; } = 180;
		
		private IEventDispatcher EventDispatcher { get; }
		public ChatComponent(IEventDispatcher eventDispatcher)
		{
			EventDispatcher = eventDispatcher;
			Anchor = Alignment.BottomLeft;

			MaxHeight = Height;
			Height = 180;
			Width = 320;

			eventDispatcher.RegisterEvents(this);

			Font = Alex.Instance.GuiRenderer.Font;
		}

		private IFont Font;

		protected override void OnInit(IGuiRenderer renderer)
		{
			Font = renderer.Font;

			FocusOutlineThickness = Thickness.Zero;
			HighlightOutlineThickness = Thickness.Zero;

			BackgroundOverlay = Color.Transparent;
			FocusOutlineColor = Color.Transparent;
			HighlightOutlineColor = Color.Transparent;

			base.TextElement.Anchor = Alignment.BottomLeft;

			BorderColor = Color.Transparent;
		}

		protected override void OnFocusActivate()
		{
			Height = FocusedHeight;
			Alex.Instance.IsMouseVisible = true;

			TextBuilder.Clear();
			Value = string.Empty;
			
			ResetTabComplete();

			TextElement.Text = string.Empty;
			Dismiss();
		}

		protected override void OnFocusDeactivate()
		{
			Height = UnfocusedHeight;
			Alex.Instance.IsMouseVisible = false;

			TextBuilder.Clear();
			Value = string.Empty;
			ResetTabComplete();

			TextElement.Text = string.Empty;
			Dismiss();
		}

		private TimeSpan _renderTimeout = TimeSpan.FromSeconds(30);
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			base.OnDraw(graphics, gameTime);

			var targetHeight = Focused ? FocusedHeight : UnfocusedHeight;
			
			if (Focused)
			{
				var renderPos = (TextElement.RenderBounds.BottomLeft() - new Vector2(0, 8)).ToPoint();
				graphics.FillRectangle(new Rectangle(RenderBounds.X, (renderPos.Y) - 2, Width, 10), new Color(Color.Black, 0.5f));
			}

			var messages = _chatEntries.ReadDeck();
			if (messages.Length > 0)
			{
				DateTime now = DateTime.UtcNow;
				Vector2 offset = new Vector2(0, 48);

				foreach (var msg in messages.Reverse())
				{
					var elapse = now - msg.time;
					float alpha = 1f;
					if (!Focused)
					{
						if (elapse > _renderTimeout)
						{
							continue;
						}

						alpha = (float) (1f - ((elapse.TotalMilliseconds / _renderTimeout.TotalMilliseconds) * 1f));
					}

					DrawChatLine(graphics, msg.message, alpha, ref offset);

					if (offset.Y - 48f >= targetHeight)
						break;
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

				bool insertColorChar = false;
				for (int i = 0; i < output.Count; i++)
				{
					if (insertColorChar)
					{
						output[i] = "§" + output[i];
					}

					string v = output[i];
					if (v.EndsWith($"§"))
					{
						insertColorChar = true;
					}
				}

				return output.ToArray();
			}
			else
			{
				return new[] {text};
			}
		}

		private void DrawChatLine(GuiSpriteBatch graphics, string text, float alpha, ref Vector2 offset)
		{
			var size = Font.MeasureString(text);

			var renderPos = Bounds.BottomLeft() - offset;

			graphics.FillRectangle(new Rectangle(renderPos.ToPoint(), new Point(Width, (int) Math.Ceiling(size.Y + 2))),
				new Color(Color.Black, alpha * 0.5f));

			Font.DrawString(graphics.SpriteBatch, text, renderPos + new Vector2(0, 2), TextColor.White, opacity: alpha);
			offset.Y += (size.Y + 2);
		}

		public static TextColor FindLastColor(string message)
		{
			TextColor last = TextColor.White;
			for (int i = 0; i < message.Length - 1; i++)
			{
				if (message[i] == '§')
				{
					last = TextColor.GetColor(message[i + 1]);
					i++;
				}
			}

			return last;
		}

		private int _latestTransactionId = -1;
		private int _tabCompletePosition = 0;
		private LinkedListNode<string> _currentNode = null;
		protected override bool OnKeyInput(char character, Keys key)
		{
			if (Focused)
			{
				/*if (key == Keys.Tab)
				{
					if (_hasTabCompleteResults)
					{
						DoTabComplete(true);
						_prevWasTab = true;
						return true;
					}

					if (TextBuilder.Length == 0) return true;

					TextBuilder.CursorPosition = 1;
					string text = TextBuilder.GetAllBehindCursor(out _tabCompletePosition);
					if (text.StartsWith('/'))
					{
						_tabCompletePosition += 1;
						text = text.Substring(1, text.Length - 1);
					}

					ChatProvider?.RequestTabComplete(text, out _latestTransactionId);
					return true;
				}
				else */if (key == Keys.Enter)
				{
					SubmitMessage();
					ResetTabComplete();
				}
				else if (key == Keys.Up && _currentNode != null)
				{
					if (_submittedMessages.Last != null && _currentNode != _submittedMessages.Last)
					{
						_currentNode = _submittedMessages.Last;
					}
					else if (_currentNode.Previous != null)
					{
						_currentNode = _currentNode.Previous;
					}

					TextBuilder.Clear();
					TextBuilder.Append(_currentNode.Value);					
				}
				else if (key == Keys.Down && _currentNode != null)
				{
					var next = _currentNode.Next;

					if (next != null)
					{
						TextBuilder.Clear();
						TextBuilder.Append(next.Value);
					}
				}
				else
				{
					int prevLength = TextBuilder.Length;
					base.OnKeyInput(character, key);
					if (TextBuilder.Length != prevLength)
					{
						ResetTabComplete();
					}
				}
				return true;
			}

			return false;
		}

		private void ResetTabComplete()
		{
			if (_hasTabCompleteResults)
			{
				_hasTabCompleteResults = false;
				_tabCompletePrevLength = 0;
			}
		}

		private bool _prevWasTab = false;
		private int _tabCompleteMatchIndex = 0;
		private int _tabCompletePrevLength = 0;
		private void DoTabComplete(bool incremental)
		{
			if (_hasTabCompleteResults)
			{
				string firstMatch = _tabCompleteMatches[_tabCompleteMatchIndex].Match;
				//_textBuilder.CursorPosition = _tabCompletePosition + _tabCompleteStart + _tabCompleteLength;

				if (_tabCompletePrevLength > 0)
				{
					TextBuilder.CursorPosition = _tabCompletePosition + _tabCompleteStart + _tabCompletePrevLength;
					for (int i = 0; i < _tabCompletePrevLength; i++)
					{
						TextBuilder.RemoveCharacter();
					}
				}
				else
				{
					TextBuilder.CursorPosition = _tabCompletePosition + _tabCompleteStart + _tabCompleteLength;
					for (int i = 0; i < _tabCompleteLength; i++)
					{
						TextBuilder.RemoveCharacter();
					}
				}

				TextBuilder.CursorPosition = _tabCompletePosition + _tabCompleteStart;
				TextBuilder.AppendLine(firstMatch);
				_tabCompletePrevLength = firstMatch.Length;

				if (incremental)
				{
					if (_tabCompleteMatchIndex + 1 >= _tabCompleteMatches.Length)
					{
						_tabCompleteMatchIndex = 0;
					}
					else
					{
						_tabCompleteMatchIndex++;
					}
				}
				else
				{
					if (_tabCompleteMatchIndex - 1 < 0)
					{
						_tabCompleteMatchIndex = _tabCompleteMatches.Length - 1;
					}
					else
					{
						_tabCompleteMatchIndex--;
					}
				}

				return;
			}
		}

		public void MoveCursor(bool add)
		{
			if (add)
			{
				TextBuilder.CursorPosition += 1;
			}
			else
			{
				TextBuilder.CursorPosition -= 1;
			}
		}

		public void Dismiss()
		{
			//Enabled = false;
			//Focused = false;

			TextBuilder.Clear();
			Value = string.Empty;

			TextElement.Text = string.Empty;
		}

		private LinkedList<string> _submittedMessages = new LinkedList<string>();

		private void SubmitMessage()
		{
			//Submit message
			if (TextBuilder.Length > 0)
			{
				if (Alex.IsMultiplayer)
				{
					EventDispatcher.DispatchEvent(
						new ChatMessagePublishEvent(new ChatObject(TextBuilder.Text)));
				}
				else
				{
					Receive(new ChatObject(TextBuilder.Text));
				}

				_submittedMessages.AddLast(TextBuilder.Text);
				_currentNode = _submittedMessages.Last;
			}

			Dismiss();
		}

		[EventHandler]
		private void OnChatMessageReceived(ChatMessageReceivedEvent e)
		{
			if (!e.IsChat())
				return;
			
			Receive(e.ChatObject);
		}

		private void Receive(ChatObject message)
		{
			string msg = message.RawMessage;

			TextColor lastColor = TextColor.White;
			var lines = CalculateLines(msg).Reverse().ToArray();
			for (var index = 0; index < lines.Length; index++)
			{
				var line = lines[index];
				if (lastColor != TextColor.White)
				{
					line = $"§{lastColor.Code}{line}";
				}

				lastColor = FindLastColor(line);

				_chatEntries.Push((line, DateTime.UtcNow));
			}
		}

		private int _tabCompleteStart, _tabCompleteLength;
		private TabCompleteMatch[] _tabCompleteMatches;
		private bool _hasTabCompleteResults = false;
		public void ReceivedTabComplete(int transactionId, int start, int length, TabCompleteMatch[] matches)
		{
			if (_latestTransactionId == transactionId)
			{
				if (matches.Length == 0) return;

				_tabCompletePrevLength = 0;
				_tabCompleteMatchIndex = 0;
				_tabCompleteMatches = matches;
				_tabCompleteStart = start;
				_tabCompleteLength = length;

				
				_hasTabCompleteResults = true;
				DoTabComplete(true);
			}
		}

		public void Unload()
		{
			EventDispatcher.UnregisterEvents(this);
		}
	}
}

