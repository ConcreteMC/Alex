using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements
{
	public class ScoreboardElement : GuiElement
	{
		private GuiTextElement Left { get; }
		private GuiTextElement Right { get; }
		
		public ScoreboardElement(string left, uint value)
		{
			Left = new GuiTextElement()
			{
				Text = left,
				Anchor = Alignment.TopLeft,
				Margin = new Thickness(0, 0, 2, 0)
			};

			Right = new GuiTextElement()
			{
				Anchor = Alignment.TopRight,
				Text = value.ToString()
			};
			
			AddChild(Left);
			AddChild(Right);
		}	
		
	/*	protected override void GetPreferredSize(out Size size, out Size minSize, out Size maxSize)
		{
			base.GetPreferredSize(out size, out minSize, out maxSize);
			
			size = new Size(Left.Width + Right.Width, Math.Max(Left.Height, Right.Height));
		}*/
	}
	
	public class ScoreboardView : GuiStackContainer
	{
		private ConcurrentDictionary<string, EntryData> Rows { get; set; } = new ConcurrentDictionary<string, EntryData>();
		public ScoreboardView() : base()
		{
			BackgroundOverlay = new Color(Color.Black, 0.5f);
			ChildAnchor = Alignment.Fill;
		}

		public void Clear()
		{
			Rows.Clear();
			foreach (var child in Children.ToArray())
			{
				RemoveChild(child);
			}
		}
		
		public void AddString(string text)
		{
			GuiContainer container = new GuiContainer();
			container.AddChild(new GuiTextElement(text)
			{
				Anchor = Alignment.CenterX
			});
			AddChild(container);
		}

		public void Remove(string id)
		{
			if (Rows.TryRemove(id, out var old))
			{
				RemoveChild(old.Container);
			}
		}

		public void AddRow(string id, string key, uint value)
		{
			if (Rows.TryRemove(id, out var old))
			{
				RemoveChild(old.Container);
			}
			
			/*GuiTextElement keyElement, valueElement;
			keyElement = new GuiTextElement()
			{
				 Text = key,
				 Anchor = Alignment.TopLeft
			};
			
			valueElement = new GuiTextElement()
			{
				 Text = value.ToString(),
				 Anchor = Alignment.TopRight
			};
			
			var container = new GuiContainer();
			GuiMultiStackContainer stackContainer = new GuiMultiStackContainer()
			{
				Orientation = Orientation.Horizontal,
				Anchor = Alignment.Fill,
				ChildAnchor = Alignment.FillCenter
			};
			stackContainer.AddRow(
				r =>
				{
					r.ChildAnchor = Alignment.Fill;
					
					var c = new GuiContainer();
					c.AddChild(keyElement);
					
					r.AddChild(c);
				});
			stackContainer.AddRow(
				r =>
				{
					r.ChildAnchor = Alignment.Fill;
					
					var c = new GuiContainer();
					c.AddChild(valueElement);
					
					r.AddChild(c);
				});
			
			container.AddChild(stackContainer);*/

			var container = new ScoreboardElement(key, value);

			//AddChild(container);
			if (Rows.TryAdd(id, new EntryData() {Container = container, Score = value}))
			{
				var rows = Rows.ToArray();
				
				foreach (var child in rows)
				{
					RemoveChild(child.Value.Container);
				}
				
				foreach (var row in rows.OrderBy(x => x.Value.Score))
				{
					AddChild(row.Value.Container);
				}
			}
			/*
			var row = AddRow(keyElement = new GuiTextElement()
				{
					Text = key,
					Anchor = Alignment.MiddleLeft
				},
				valueElement = new GuiTextElement()
				{
					Text = value,
					Anchor = Alignment.MiddleRight
				});
*/
			//	row.Anchor = Alignment.TopFill;
			//keyElement.Anchor = Alignment.TopLeft;
			//	valueElement.Anchor = Alignment.TopRight;
			//	var keyElementMargin = keyElement.Margin;
			//	keyElementMargin.Left = 0;

			//	keyElement.Margin = keyElementMargin;

			//	var valueElementMargin = valueElement.Margin;
			//		valueElementMargin.Right = 0;

			//		valueElement.Margin = valueElementMargin;
			//keyElement.Anchor = Alignment.MinX;
			//valueElement.Anchor = Alignment.MaxX;

		}

		private class EntryData
		{
			public ScoreboardElement Container { get; set; }
			public uint Score { get; set; }
		}
	}
}
