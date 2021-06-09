using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Common.Data;
using Alex.Common.Graphics.Typography;
using Alex.Common.Utils;
using Alex.Net;
using Alex.Utils;
using Alex.Utils.Commands;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MiNET;
using MiNET.Plugins;
using NLog;
using RocketUI;
using SixLabors.ImageSharp.Drawing;

using MessageType = Alex.Common.Data.MessageType;

namespace Alex.Gui.Elements
{
	public class ChatComponent : TextInput, IChatRecipient
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChatComponent));
		
		private ConcurrentDeck<(string message, DateTime time)> _chatEntries = new ConcurrentDeck<(string message, DateTime time)>(10);

		public int UnfocusedHeight { get; set; } = 100;
		public int FocusedHeight { get; set; } = 180;
		
		public  NetworkProvider  Network         { get; set; }
		public CommandProvider CommandProvider { get; set; } = null;

		private static ChatComponent _instance;
		public ChatComponent()
		{
			Anchor = Alignment.BottomLeft;

			MaxHeight = Height;
			Height = 180;
			Width = 320;
			
			Font = Alex.Instance.GuiRenderer.Font;

			Padding = new Thickness(2, 0, 2, 0);
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
			
			_instance = this;
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
				Vector2 offset = new Vector2(0, -48);

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

					if (alpha <= 0)
						break;
					
					DrawChatLine(graphics, msg.message, alpha, ref offset);

					if (offset.Y - 48f >= targetHeight)
						break;
				}
			}

			//if (Focused)
			{
				if (_hasTabCompleteResults && _tabCompleteMatches.Length > 0)
				{
					var tabRenderPos = (TextElement.RenderBounds.BottomLeft() - new Vector2(0, 8));

					var t = TextBuilder.Text;
					var tabCompleteStart = _tabCompleteStart;

					var stringOffset = t.Substring(0, tabCompleteStart);
					var stringSize = Font.MeasureString(stringOffset);

					var xOffset = stringSize.X + TextElement.Margin.Left;
					tabRenderPos.X += xOffset;
					
					var offset = _tabCompleteMatchIndex;
					var tabCompletionResults = _tabCompleteMatches;
					var maxWidth = tabCompletionResults.Max(x => x.Size.X);
					for(int i = 0; i < tabCompletionResults.Length; i++)
					{
						var tab = tabCompletionResults[i];
						var text = tab.Match.GetDescriptive();
						var size = tab.Size;
						tabRenderPos.Y -= size.Y;

						graphics.FillRectangle(new Rectangle((int) tabRenderPos.X, (int) (tabRenderPos.Y) - 2, maxWidth, size.Y), new Color(Color.Black, 0.95f));
						graphics.DrawString(tabRenderPos, text, i == (offset - 1) ? Color.Gold : Color.White, FontStyle.None, 1f);
					}

					stringOffset = t.Substring(0, TextBuilder.CursorPosition);
					stringSize = Font.MeasureString(stringOffset);
					
					var currentSelection = tabCompletionResults[offset];
					if (currentSelection.Match.HasTooltip)
					{
						graphics.DrawString(Font, currentSelection.Match.Tooltip, new Vector2(TextElement.RenderBounds.BottomLeft().X + TextElement.Margin.Left + stringSize.X, TextElement.RenderBounds.Y + TextElement.Margin.Top), Common.Utils.TextColor.Gray, FontStyle.None);
					}
				}
			}
		}

		private double MaxTextWidth => (RenderBounds.Width - (Padding.Left + Padding.Right));
		private string GetFitting(string text, out string rest)
		{
			rest = string.Empty;

			var size = Font.MeasureString(text);
			while ((size.X) > MaxTextWidth)
			{
				string current        = text;

				if (current.Length == 0)
					break;
				
				var lastWhiteSpace = current.LastIndexOf(' ');
				if (lastWhiteSpace > 0)
				{
					text = current.Remove(lastWhiteSpace, current.Length - lastWhiteSpace);
                    rest = current.Substring(lastWhiteSpace, current.Length - lastWhiteSpace) + rest;
				}
				else
				{
					text = current.Remove(current.Length - 1, 1);
					rest = current.Substring( current.Length - 1, 1) + rest;
				}

				size = Font.MeasureString(text);
			}

			return text;
		}

		private string[] CalculateLines(string text)
		{
			var size = Font.MeasureString(text);
			if (size.X > MaxTextWidth)
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

			var renderPos = RenderBounds.BottomLeft() + offset;

			graphics.FillRectangle(new Rectangle(renderPos.ToPoint(), new Point(Width, (int) Math.Ceiling(size.Y + 2))),
				new Color(Color.Black, alpha * 0.5f));

			Font.DrawString(graphics.SpriteBatch, text, renderPos + new Vector2(Padding.Left, 2), (Color) Common.Utils.TextColor.White, opacity: alpha);
			offset.Y -= (size.Y + 2);
		}

		public static TextColor FindLastColor(string message)
		{
			TextColor last = Common.Utils.TextColor.White;
			for (int i = 0; i < message.Length - 1; i++)
			{
				if (message[i] == '§')
				{
					last = Common.Utils.TextColor.GetColor(message[i + 1]);
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
				if (key == Keys.Tab && CommandProvider != null)
				{
					//ResetTabComplete();
					if (_hasTabCompleteResults)
					{
						DoTabComplete(true);
						//_prevWasTab = true;
						return true;
					}

					if (TextBuilder.Length == 0) return true;

					//TextBuilder.CursorPosition = 1;
					_tabCompletePosition = TextBuilder.CursorPosition;
					string text = TextBuilder.Text.Substring(0, TextBuilder.CursorPosition);//.GetAllBehindCursor(out _tabCompletePosition);
					if (text.StartsWith('/'))
					{
						_tabCompletePosition += 1;
						text = text.Substring(1, text.Length - 1);
					}

					CommandProvider.Match(text, ReceivedTabComplete);
					//ChatProvider?.RequestTabComplete(text, out _latestTransactionId);
					return true;
				}
				else if (key == Keys.Enter)
				{
					if (_hasTabCompleteResults)
					{
						ResetTabComplete();

						return true;
					}

					SubmitMessage();
					ResetTabComplete();
					
					return true;
				}
				else if (key == Keys.Up)
				{
					if (_hasTabCompleteResults)
					{
						DoTabComplete(true);

						return true;
					}
					
					var currentNode = _currentNode;
					if (currentNode == null)
					{
						currentNode = _submittedMessages.Last;
					}
					else if (currentNode.Previous != null)
					{
						currentNode = currentNode.Previous;
					}

					if (currentNode != null && !string.IsNullOrEmpty(currentNode.Value))
					{
						TextBuilder.Clear();
						TextBuilder.Append(_currentNode.Value);
					}

					return true;
				}
				else if (key == Keys.Down)
				{
					if (_hasTabCompleteResults)
					{
						DoTabComplete(false);

						return true;
					}

					if (_currentNode != null)
					{

						var next = _currentNode.Next;

						if (next != null)
						{
							TextBuilder.Clear();
							TextBuilder.Append(next.Value);
						}

						return true;
					}
				}
				/*else
				{
					int prevLength = TextBuilder.Length;
					base.OnKeyInput(character, key);
					if (TextBuilder.Length != prevLength)
					{
						ResetTabComplete();
					}
				}*/
			}

			var position = TextBuilder.CursorPosition;
			base.OnKeyInput(character, key);
			if (TextBuilder.CursorPosition != position)
			{
				ResetTabComplete();
				return true;
			}

			return true;
		}

		private void ResetTabComplete()
		{
			if (_hasTabCompleteResults)
			{
				_hasTabCompleteResults = false;
				_tabCompletePrevLength = 0;
				_tabCompleteMatchIndex = 0;
				_tabCompletePosition = 0;
			}
		}
		
		private int _tabCompleteMatchIndex = 0;
		private int _tabCompletePrevLength = 0;
		private void DoTabComplete(bool incremental)
		{
			if (_hasTabCompleteResults)
			{
				string firstMatch = _tabCompleteMatches[_tabCompleteMatchIndex].Match.Match;
				//_textBuilder.CursorPosition = _tabCompletePosition + _tabCompleteStart + _tabCompleteLength;

				//Remove the existing match first.
				int removalLength = _tabCompletePrevLength > 0 ? _tabCompletePrevLength : _tabCompleteLength;
				TextBuilder.CursorPosition = _tabCompletePosition + _tabCompleteStart + removalLength;
				for (int i = 0; i < removalLength; i++)
				{ 
					TextBuilder.RemoveCharacter();
				}

				//Now append the new match
				TextBuilder.CursorPosition = _tabCompletePosition + _tabCompleteStart;
				TextBuilder.Append(firstMatch);
				
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
				var text = TextBuilder.Text;
				if (Alex.IsMultiplayer)
				{
					Network?.SendChatMessage(new ChatObject(text));
					//EventDispatcher.DispatchEvent(
					//	new ChatMessagePublishEvent(new ChatObject(TextBuilder.Text)));
				}
				else
				{
					Receive(TextBuilder.Text);
				}

				_currentNode = _submittedMessages.AddLast(text);

				if (_submittedMessages.Count > 10)
				{
					_submittedMessages.RemoveFirst();
				}
				//_currentNode = _submittedMessages.Last;
			}

			Dismiss();
		}

		private void Receive(string message)
		{
			string msg = message;

			TextColor lastColor = Common.Utils.TextColor.White;

			foreach (var split in msg.Split('\n'))
			{
				foreach (var line in CalculateLines(split).Reverse())
				{
					var t = line;
					if (lastColor != Common.Utils.TextColor.White)
					{
						t = $"§{lastColor.Code}{t}";
					}

					lastColor = FindLastColor(t);

					_chatEntries.Push((t, DateTime.UtcNow));
                }

				/*for (var index = 0; index < lines.Length; index++)
				{
					var line = lines[index];

					if (lastColor != TextColor.White)
					{
						line = $"§{lastColor.Code}{line}";
					}

					lastColor = FindLastColor(line);

					_chatEntries.Push((line, DateTime.UtcNow));
				}*/
			}
		}

		private int _tabCompleteStart, _tabCompleteLength;
		private TabCompleteMatchWrapper[] _tabCompleteMatches;
		private bool _hasTabCompleteResults = false;
		public void ReceivedTabComplete(int start, int length, TabCompleteMatch[] matches)
		{
		//t/he	if (_latestTransactionId == transactionId)
			{
				if (matches.Length == 0)
				{
					ResetTabComplete();
					return;
				}

				_tabCompletePrevLength = 0;
				_tabCompleteMatchIndex = 0;

				matches = matches.Distinct().ToArray();
				TabCompleteMatchWrapper[] wrappers = new TabCompleteMatchWrapper[matches.Length];
				for (int i = 0; i < matches.Length; i++)
				{
					wrappers[i] = new TabCompleteMatchWrapper(matches[i], Font.MeasureString(matches[i].GetDescriptive()).ToPoint());
				}
				_tabCompleteMatches = wrappers;
				
				_tabCompleteStart = start;
				_tabCompleteLength = length;

				
				_hasTabCompleteResults = true;
				DoTabComplete(true);
			}
		}

		public void Unload()
		{
		
		}

		/// <inheritdoc />
		public void AddMessage(string message, MessageType messageType)
		{
			Receive(message);
		}

		private class TabCompleteMatchWrapper
		{
			public TabCompleteMatch Match { get; }
			public Point Size { get; }
			public TabCompleteMatchWrapper(TabCompleteMatch tabCompleteMatch, Point size)
			{
				Match = tabCompleteMatch;
				Size = size;
			}
		}

		public static void AddSystemMessage(string message)
		{
			_instance?.AddMessage(message, MessageType.System);
		}
	}

	public interface IChatRecipient
	{
		void AddMessage(string message, MessageType messageType);
	}
}

