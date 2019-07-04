using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Alex.API.Gui.Events;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using NLog;

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

	public class GuiScrollBar : GuiControl
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

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

		private GuiButton ScrollDownButton;
		private GuiButton ScrollUpButton;
		private GuiButton Track;

		public GuiTexture2D ThumbBackground;
		public GuiTexture2D ThumbHighlightBackground;

		private Orientation _orientation     = Orientation.Vertical;
		private int         _maxScrollOffset = 0;
		private int         _scrollOffsetValue;

		public int ScrollButtonStep { get; set; } = 5;

		public int ScrollOffsetValue
		{
			get => _scrollOffsetValue;
			set
			{
				if (value == _scrollOffsetValue) return;

				var prevValue = _scrollOffsetValue;
				_scrollOffsetValue = Math.Clamp(value, 0, _maxScrollOffset);
				ScrollOffsetValueChanged?.Invoke(this, new ScrollOffsetValueChangedEventArgs(_scrollOffsetValue));

				OnScrollOffsetValueChanged(prevValue, _scrollOffsetValue);

				Log.Info($"ScrollOffsetValue.Change {{ScrollOffsetValue=({prevValue} => {_scrollOffsetValue}), ScrollButtonStep={ScrollButtonStep}, MaxScrollOffset={MaxScrollOffset}}}");
				Debug.WriteLine($"ScrollOffsetValue.Change {{ScrollOffsetValue=({prevValue} => {_scrollOffsetValue}), ScrollButtonStep={ScrollButtonStep}, MaxScrollOffset={MaxScrollOffset}}}");
			}
		}

		public int MaxScrollOffset
		{
			get => _maxScrollOffset;
			set
			{
				if (value == _maxScrollOffset) return;

				_maxScrollOffset  = value;
				ScrollOffsetValue = ScrollOffsetValue;
			}
		}

		public GuiScrollBar()
		{
			Background               = Color.Black;
			ThumbBackground          = GuiTextures.ButtonDefault;
			ThumbHighlightBackground = GuiTextures.ButtonHover;

			Background.RepeatMode               = TextureRepeatMode.NoScaleCenterSlice;
			ThumbBackground.RepeatMode          = TextureRepeatMode.NoScaleCenterSlice;
			ThumbHighlightBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;

			MinWidth  = 10;
			MinHeight = 10;

			Padding = Thickness.Zero;
			Margin  = Thickness.Zero;

			AddChild(ScrollDownButton = new GuiButton(() => ScrollOffsetValue += ScrollButtonStep)
			{
				Width  = 10,
				Height = 10,
				Margin = new Thickness(0, 0, 0, 0),

				Background            = GuiTextures.ScrollBarDownButtonDefault,
				HighlightedBackground = GuiTextures.ScrollBarDownButtonHover,
				FocusedBackground     = GuiTextures.ScrollBarDownButtonFocused,
				DisabledBackground    = GuiTextures.ScrollBarDownButtonDisabled,
			});

			AddChild(ScrollUpButton = new GuiButton(() => ScrollOffsetValue -= ScrollButtonStep)
			{
				Width  = 10,
				Height = 10,
				Margin = new Thickness(0, 0, 0, 0),

				Background            = GuiTextures.ScrollBarUpButtonDefault,
				HighlightedBackground = GuiTextures.ScrollBarUpButtonHover,
				FocusedBackground     = GuiTextures.ScrollBarUpButtonFocused,
				DisabledBackground    = GuiTextures.ScrollBarUpButtonDisabled
			});

			AddChild(Track = new GuiButton()
			{
				MinWidth  = 10,
				MinHeight = 10,
				Margin    = new Thickness(0, 0, 0, 0),

				Background            = GuiTextures.ScrollBarTrackDefault,
				HighlightedBackground = GuiTextures.ScrollBarTrackHover,
				FocusedBackground     = GuiTextures.ScrollBarTrackFocused,
				DisabledBackground    = GuiTextures.ScrollBarTrackDisabled
			});

			Track.Background.RepeatMode            = TextureRepeatMode.NoScaleCenterSlice;
			Track.HighlightedBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
			Track.FocusedBackground.RepeatMode     = TextureRepeatMode.NoScaleCenterSlice;
			Track.DisabledBackground.RepeatMode    = TextureRepeatMode.NoScaleCenterSlice;

			Track.CursorMove    += TrackOnCursorMove;
			Track.CursorPressed += TrackOnCursorPressed;

			Orientation = Orientation.Vertical;
		}

		private void TrackOnCursorPressed(object sender, GuiCursorEventArgs e)
		{
			SetValueFromCursor(e.CursorPosition);
		}

		private void TrackOnCursorMove(object sender, GuiCursorMoveEventArgs e)
		{
			if (e.IsCursorDown)
				SetValueFromCursor(e.CursorPosition);
		}

		private void SetValueFromCursor(Point relativePosition)
		{
			var percentageClicked = Orientation == Orientation.Vertical
										? (relativePosition.Y / (float) RenderBounds.Height)
										: (relativePosition.X / (float) RenderBounds.Width);

			ScrollOffsetValue = (int) Math.Round(MaxScrollOffset * percentageClicked);
		}

		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);

			ThumbBackground.TryResolveTexture(renderer);
			ThumbHighlightBackground.TryResolveTexture(renderer);
		}

		private void OnScrollOffsetValueChanged(int prevValue, int scrollOffsetValue)
		{
			UpdateTrack();
		}

		private void OnOrientationChanged()
		{
			if (Orientation == Orientation.Vertical)
			{
				ScrollDownButton.Anchor = Alignment.BottomFill;
				ScrollDownButton.Margin = new Thickness(0, 0, 0, 0);

				ScrollUpButton.Anchor = Alignment.TopFill;
				ScrollUpButton.Margin = new Thickness(0, 0, 0, 10);
			}
			else
			{
				ScrollDownButton.Anchor = Alignment.FillRight;
				ScrollDownButton.Margin = new Thickness(0, 0, 0, 0);

				ScrollUpButton.Anchor = Alignment.FillLeft;
				ScrollUpButton.Margin = new Thickness(0, 0, 10, 0);

				ScrollUpButton.Rotation   = 270f;
				ScrollDownButton.Rotation = 270f;
			}

			UpdateTrack();
		}

		private void UpdateTrack()
		{
			var containerSize = Orientation == Orientation.Vertical ? RenderBounds.Height : RenderBounds.Width;
			var trackSize = Orientation == Orientation.Vertical
								? (containerSize - ScrollUpButton.Height - ScrollDownButton.Height)
								: (containerSize - ScrollUpButton.Width - ScrollDownButton.Width);
			var maxOffset = MaxScrollOffset;

			var contentSize = containerSize + maxOffset;

			var visibleSizeAsPercentage = (containerSize / (double) contentSize);
			if (visibleSizeAsPercentage >= 1.0d)
			{
				if (Orientation == Orientation.Vertical)
					Track.Height = trackSize;
				else
					Track.Width = trackSize;

				Track.Enabled            = false;
				ScrollUpButton.Enabled   = false;
				ScrollDownButton.Enabled = false;
			}
			else
			{
				var size = (int) Math.Floor(visibleSizeAsPercentage * trackSize);


				if (Orientation == Orientation.Vertical)
					Track.Height = size;
				else
					Track.Width = size;

				Track.Enabled = true;

				ScrollUpButton.Enabled   = (ScrollOffsetValue > 0);
				ScrollDownButton.Enabled = (ScrollOffsetValue < MaxScrollOffset);
			}
		}
	}
}