using System;
using Alex.Common.Gui.Graphics;
using Alex.Common.Utils;
using Alex.Entities;
using Alex.Interfaces;
using Microsoft.Xna.Framework;
using RocketUI;
using RectangleExtensions = RocketUI.Utilities.Extensions.RectangleExtensions;

namespace Alex.Gui.Elements.Hud
{
	public class ExperienceComponent : Container
	{
		private Player Player { get; }

		private float Experience { get; set; }
		private float ExperienceLevel { get; set; }

		private string _text;

		private bool _sizeDirty = true;
		private Vector2 _textSize = Vector2.Zero;

		private ITexture2D _bg;

		public ExperienceComponent(Player player)
		{
			Player = player;

			Background = AlexGuiTextures.ExperienceBackground;
			BackgroundOverlay = AlexGuiTextures.Experience;

			Height = 5;
			//AutoSizeMode = AutoSizeMode.None;
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
			_bg = renderer.GetTexture(AlexGuiTextures.Experience);

			Background.Scale = new Vector2(182f / Background.Width, 5f / Background.Height);
			BackgroundOverlay.Scale = new Vector2(182f / BackgroundOverlay.Width, 5f / BackgroundOverlay.Height);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			if (Math.Abs(Player.Experience - Experience) > 0.001f)
			{
				Experience = Player.Experience;
				var source = _bg.ClipBounds.Location;
				var sourceSize = _bg.ClipBounds.Size;

				BackgroundOverlay = _bg.Texture.Slice(
					source.X, source.Y, (int)(Experience * sourceSize.X), sourceSize.Y);

				BackgroundOverlay.Scale = new Vector2(182f / BackgroundOverlay.Width, 5f / BackgroundOverlay.Height);
			}

			if (Math.Abs(Player.ExperienceLevel - ExperienceLevel) > 0.001f)
			{
				ExperienceLevel = Player.ExperienceLevel;
				_text = $"{(int)ExperienceLevel}";
				_sizeDirty = true;
			}
		}

		/// <inheritdoc />
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			graphics.FillRectangle(RenderBounds, Background);

			//var source = BackgroundOverlay.Texture.ClipBounds.Location;
			//var size = BackgroundOverlay.Texture.ClipBounds.Size;

			graphics.FillRectangle(RenderBounds, BackgroundOverlay);

			if (_sizeDirty)
			{
				_sizeDirty = false;
				_textSize = graphics.Font.MeasureString(_text);
			}

			if (ExperienceLevel >= 1f)
				graphics.DrawString(
					RectangleExtensions.BottomCenter(RenderBounds) - new Vector2(_textSize.X / 2f, _textSize.Y), _text,
					TextColor.BrightGreen.ForegroundColor, FontStyle.DropShadow, 1f);
		}
	}
}