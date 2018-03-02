using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Common;
using Alex.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui
{
	public class UiElement
	{
		public UiContainer Container { get; internal set; }

		public UiSkin Skin { get; set; }

		public Rectangle Bounds { get; private set; }
		public Rectangle ClientBounds { get; private set; }

		public Point Position { get; set; }
		public Point Size { get; set; }

		public int? Width { get; set; } = null;
		public int? Height { get; set; } = null;

		public bool Visible { get; set; }

		#region Styling Properties

		public Thickness Margin { get; set; } = Thickness.Zero;
		public Thickness Padding { get; set; } = Thickness.Zero;

		public Color BackgroundColor { get; set; } = Color.Transparent;
		public Texture2D BackgroundImage { get; set; } = null;

		public Color ForegroundColor { get; set; } = Color.White;

		public Color BorderColor { get; set; } = Color.Transparent;
		public Thickness BorderWidth { get; set; } = Thickness.Zero;

		#endregion

		public UiElement(int? width, int? height)
		{
			Width = width;
			Height = height;
		}

		public UiElement()
		{

		}

		internal void ApplySkin(UiSkin skin)
		{
			var style = skin.GetStyle(GetType().FullName.ToLowerInvariant());
			//ApplyStyle(style);

			OnApplySkin(skin);
		}

		internal void ApplyStyle(UiElementStyle style)
		{
			BackgroundColor = style.Background;
			ForegroundColor = style.Foreground;
			BorderColor = style.BorderColor;
			BorderWidth = style.BorderWidth;
			Margin = style.Margin;
			Padding = style.Thickness;
		}

		protected internal virtual void OnApplySkin(UiSkin skin)
		{

		}

		public void UpdateLayout()
		{
			Bounds = new Rectangle(Position, Size);
			ClientBounds = new Rectangle(Bounds.X + Padding.Left, Bounds.Y + Padding.Top, Bounds.Width - Padding.Left - Padding.Right, Bounds.Height - Padding.Top - Padding.Bottom);
		}

		public void Draw(GameTime gameTime, GuiRenderer renderer)
		{
			OnDraw(gameTime, renderer);
		}

		public void Update(GameTime gameTime)
		{
			OnUpdate(gameTime);
		}

		protected virtual void OnDraw(GameTime gameTime, GuiRenderer renderer)
		{
			// Border
			DrawBorder(renderer);

			// Background
			DrawBackground(renderer);
		}

		protected virtual void OnUpdate(GameTime gameTime)
		{

		}


		private void DrawBorder(GuiRenderer renderer)
		{

		}

		private void DrawBackground(GuiRenderer renderer)
		{
			if (BackgroundColor.A > 0)
			{
				renderer.DrawRectangle(ClientBounds, BackgroundColor);
			}

			if (BackgroundImage != null)
			{
				renderer.DrawRectangle(ClientBounds, BackgroundImage);
			}
		}
	}
}
