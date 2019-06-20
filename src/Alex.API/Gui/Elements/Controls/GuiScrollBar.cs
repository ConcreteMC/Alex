using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
	public class ScrollOffsetValueChangedEventArgs : EventArgs
	{
		public int ScrollOffsetValue { get; }

		internal ScrollOffsetValueChangedEventArgs(int scrollOffsetValue)
		{
			ScrollOffsetValue = scrollOffsetValue;
		}
	}

	public class GuiScrollBar : GuiElement
	{
		public event EventHandler<ScrollOffsetValueChangedEventArgs> ScrollOffsetValueChanged; 

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
		private int _maxScrollOffset = 0;
		private int _scrollOffsetValue;

		public int ScrollButtonStep { get; set; } = 5;

		public int ScrollOffsetValue
		{
			get => _scrollOffsetValue;
			set
			{
				_scrollOffsetValue = Math.Clamp(value, 0, _maxScrollOffset);
				ScrollOffsetValueChanged?.Invoke(this, new ScrollOffsetValueChangedEventArgs(_scrollOffsetValue));
			}
		}

		public int MaxScrollOffset
		{
			get => _maxScrollOffset;
			set
			{
				_maxScrollOffset = value;
				ScrollOffsetValue = Math.Clamp(ScrollOffsetValue, 0, _maxScrollOffset);
			}
		}

		public GuiScrollBar()
		{
			Background = Color.Black;
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
				Height = 10,
				Margin = new Thickness(0, 0, 0, 0),
				Background = Color.MediumVioletRed
			});
			AddChild(ScrollIncreaseButton = new GuiButton(() => ScrollOffsetValue += ScrollButtonStep)
			{
				Width = 10,
				Height = 10,
				Margin = new Thickness(0, 0, 0, 0),
				Background = Color.CornflowerBlue
			});

			Orientation = Orientation.Vertical;
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
				ScrollDecreaseButton.Margin = new Thickness(0, 0, 0, 0);
				ScrollIncreaseButton.Anchor = Alignment.TopFill;
				ScrollIncreaseButton.Margin = new Thickness(0, 0, 0, 10);
			}
			else
			{
				ScrollDecreaseButton.Anchor = Alignment.FillRight;
				ScrollDecreaseButton.Margin = new Thickness(0, 0, 0, 0);
				ScrollIncreaseButton.Anchor = Alignment.FillLeft;
				ScrollIncreaseButton.Margin = new Thickness(0, 0, 10, 0);
			}
		}
	}
}
