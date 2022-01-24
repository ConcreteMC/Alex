using System;
using System.Threading;
using Alex.Common.Gui.Elements;
using Alex.Common.Utils;
using Alex.Common.World;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Attributes;


namespace Alex.Gui.Elements
{
	public class TitleComponent : StackContainer, ITitleComponent, ITicked
	{
		private float _fadeValue = 1.0f;
		private TextElement _title;
		private TextElement _subTitle;

		public TitleComponent()
		{
			Anchor = Alignment.MiddleFill;
			Orientation = Orientation.Vertical;

			_title = new TextElement()
			{
				//Anchor = Alignment.TopCenter,
				TextColor = (Color)TextColor.White, FontStyle = FontStyle.DropShadow, Scale = 2f, Text = ""
			};

			_subTitle = new TextElement()
			{
				//Anchor = Alignment.MiddleCenter,
				TextColor = (Color)TextColor.White, FontStyle = FontStyle.None, Scale = 1f, Text = ""
			};

			AddChild(_title);
			AddChild(_subTitle);
		}

		public void Ready()
		{
			Show();
		}

		#region Titles

		private int _fadeInTicks = 0, _fadeOutTicks = 0, _displayTicks = 20;

		public void Show()
		{
			// _fadeInTicks = _fadeIn;
			//  _fadeOutTicks = _fadeOut;
			//  _displayTicks = _displayTime;

			_fadeIn = 0;
			_fadeOut = 0;
			_displayTime = 0;

			IsVisible = true;
			//_hideTime = DateTime.UtcNow + TimeSpan.FromMilliseconds((_fadeIn + _fadeOut + _stay) * 50);

			//AddChild(_title);
			// AddChild(_subTitle);
		}

		public void SetTitle(string value)
		{
			_title.Text = value;
		}

		public void SetSubtitle(string value)
		{
			_subTitle.Text = value;
		}


		private int _fadeIn = 0, _fadeOut = 0, _displayTime = 20;

		public void SetTimes(int fadeIn, int stay, int fadeOut)
		{
			_displayTicks = Math.Max(Math.Min(stay, 200), 20); //10 Seconds max
			_fadeInTicks = fadeIn >= 0 ? fadeIn : 0;
			_fadeOutTicks = fadeOut >= 0 ? fadeOut : 0;
		}

		public void Hide()
		{
			Reset();

			IsVisible = false;

			//RemoveChild(_title);
			//RemoveChild(_subTitle);
		}

		public void Reset()
		{
			_title.Text = string.Empty;
			_subTitle.Text = string.Empty;

			//_fadeInTicks = 0; 
			// _fadeOutTicks = 0;
			//  _displayTicks = 20;
			_fadeIn = 0;
			_fadeOut = 0;
			_displayTime = 0;
		}

		#endregion

		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			if (!IsVisible) return;

			if (_fadeIn < _fadeInTicks)
			{
				var progress = (1f / _fadeInTicks) * _fadeIn;
				_title.TextOpacity = _subTitle.TextOpacity = progress;
			}
			else if (_fadeOut < _fadeOutTicks && _displayTime >= _displayTicks)
			{
				var progress = 1f - (1f / _fadeOutTicks) * _fadeOut;
				_title.TextOpacity = _subTitle.TextOpacity = progress;
			}
			else
			{
				_title.TextOpacity = _subTitle.TextOpacity = 1f;
			}

			base.OnDraw(graphics, gameTime);
		}

		/// <inheritdoc />
		public void OnTick()
		{
			if (!IsVisible) return;

			if (_fadeIn < _fadeInTicks)
			{
				_fadeIn++;
			}
			else if (_fadeIn >= _fadeInTicks)
			{
				if (_displayTime < _displayTicks)
				{
					_displayTime++;
				}
				else if (_fadeOut < _fadeOutTicks)
				{
					_fadeOut++;
				}
			}

			if (ElapsedTicks >= TargetTicks - 1)
				Hide();
		}

		[DebuggerVisible] public int ElapsedTicks => Math.Abs(_fadeIn + _fadeOut + _displayTime);
		[DebuggerVisible] public int TargetTicks => Math.Abs(_fadeInTicks + _fadeOutTicks + _displayTicks);
	}
}