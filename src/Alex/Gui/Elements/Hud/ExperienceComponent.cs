using System;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Entities;
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
		public ExperienceComponent(Player player )
		{
			Player = player;
			
			Background = AlexGuiTextures.ExperienceBackground;
			BackgroundOverlay = AlexGuiTextures.Experience;
			
			Height = 5;
			//AutoSizeMode = AutoSizeMode.None;
		}

		
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			if (Math.Abs(Player.Experience - Experience) > 0.001f)
			{
				Experience = Player.Experience;
				BackgroundOverlay.Scale = new Vector2(Experience, 1f);
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
			base.OnDraw(graphics, gameTime);

			if (_sizeDirty)
			{
				_sizeDirty = false;
				_textSize = graphics.Font.MeasureString(_text);
			}
			
			if (ExperienceLevel >= 1f)
				graphics.DrawString(RectangleExtensions.BottomCenter(RenderBounds) - new Vector2(_textSize.X / 2f, _textSize.Y), _text, TextColor.BrightGreen.ForegroundColor, FontStyle.DropShadow, 1f);
		}
	}
}