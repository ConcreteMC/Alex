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
			
			//ChildAnchor = Alignment.TopLeft;
		}

		protected override void ArrangeChildrenCore(Rectangle finalRect, IReadOnlyCollection<GuiElement> children)
		{
			var positioningBounds = finalRect;

			var alignment = NormalizeAlignmentForArrange(Orientation, ChildAnchor);

			var offset = Thickness.Zero;
			var lastOffset = Thickness.Zero;
			var childSize = Thickness.Zero;


			foreach (var child in children)
			{
				
				//positioningBounds -= offset;
				//var alignment = NormalizeAlignmentForArrange(Orientation, child.Anchor);
				//var alignment = child.Anchor;
				//alignment = child.Anchor;
				var layoutBounds = PositionChild(child, alignment, positioningBounds, lastOffset, offset, true);

				var currentOffset = CalculateOffset(alignment, layoutBounds.Size, layoutBounds.Margin, lastOffset);

				offset += currentOffset;

				

				lastOffset = CalculateOffset(alignment, Size.Zero, layoutBounds.Margin, lastOffset);
			}
		}

		protected override void OnInit(IGuiRenderer renderer)
		{
			
			base.OnInit(renderer);
		}

		public void AddString(string text)
		{
			AddRow(string.Empty, text);
		}

		public void AddRow(string key, string value)
		{
			GuiTextElement keyElement, valueElement;
			var row = AddRow(keyElement = new GuiTextElement()
				{
					Text = key,

				},
				valueElement = new GuiTextElement()
				{
					Text = value
				});

			//	row.Anchor = Alignment.TopFill;
			keyElement.Anchor = Alignment.TopLeft;
				valueElement.Anchor = Alignment.TopRight;
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
			var stack = new GuiStackContainer()
			{
				Orientation = Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal,
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
