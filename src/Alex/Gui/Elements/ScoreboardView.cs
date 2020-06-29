using System.Collections.Generic;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements
{
	public class ScoreboardView : GuiStackContainer
	{
		public ScoreboardView() : base()
		{
			BackgroundOverlay = new Color(Color.Black, 0.5f);
			ChildAnchor = Alignment.Fill;
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

		public void AddRow(string key, string value)
		{
			GuiTextElement keyElement, valueElement;
			keyElement = new GuiTextElement()
			{
				 Text = key,
				 Anchor = Alignment.TopLeft
			};
			
			valueElement = new GuiTextElement()
			{
				 Text = value,
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
			
			container.AddChild(stackContainer);
			
			AddChild(container);
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

		private GuiStackContainer AddRow(params GuiElement[] elements)
		{
			var stack = new GuiMultiStackContainer()
			{
				Orientation = Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal,
				Anchor = Alignment.FillX
				//Anchor = Alignment.TopLeft
			};

			AddChild(stack);

		//	stack.Anchor = Alignment.TopFill;
			foreach (var element in elements)
			{
				stack.AddChild(element);
			}

			return stack;
		}
	}
}
