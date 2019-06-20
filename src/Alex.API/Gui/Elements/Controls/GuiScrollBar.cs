using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Graphics;

namespace Alex.API.Gui.Elements.Controls
{
	public class GuiScrollBar : GuiElement
	{
		public Orientation Orientation
		{
			get => _orientation;
			set
			{
				_orientation = value;
				OnOrientationChanged();
			}
		}

		private GuiButton ScrollDecreaseButton;
		private GuiButton ScrollIncreaseButton;
		
		public GuiTexture2D ThumbBackground;
		public GuiTexture2D ThumbHighlightBackground;
		private Orientation _orientation = Orientation.Vertical;

		public int ScrollButtonStep { get; set; } = 5;
		public int ScrollOffsetValue { get; set; }

		public GuiScrollBar()
		{
			Background = GuiTextures.ButtonDisabled;
			ThumbBackground = GuiTextures.ButtonDefault;
			ThumbHighlightBackground = GuiTextures.ButtonHover;
            
			Background.RepeatMode               = TextureRepeatMode.NoScaleCenterSlice;
			ThumbBackground.RepeatMode          = TextureRepeatMode.NoScaleCenterSlice;
			ThumbHighlightBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;

			MinWidth  = 10;
			MinHeight = 10;

			Padding   = Thickness.Zero;
			Margin    = Thickness.Zero;

			AddChild(ScrollDecreaseButton = new GuiButton(() => ScrollOffsetValue -= ScrollButtonStep)
			{
				Width = 10,
				Height = 10
			});
			AddChild(ScrollIncreaseButton = new GuiButton(() => ScrollOffsetValue += ScrollButtonStep)
			{
				Width = 10,
				Height = 10
			});
		}
		
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);

			ThumbBackground.TryResolveTexture(renderer);
			ThumbHighlightBackground.TryResolveTexture(renderer);
		}

		private void OnOrientationChanged()
		{
			if (Orientation == Orientation.Vertical)
			{
				ScrollDecreaseButton.Anchor = Alignment.BottomFill;
				ScrollIncreaseButton.Anchor = Alignment.TopFill;
			}
			else
			{
				ScrollDecreaseButton.Anchor = Alignment.FillRight;
				ScrollIncreaseButton.Anchor = Alignment.FillLeft;
			}
		}
	}
}
