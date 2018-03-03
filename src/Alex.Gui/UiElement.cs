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

		public Point Offset { get;set; }

		public Point Position { get; set; }
		public Point Size => new Point(ActualWidth, ActualHeight);

		public int? Width { get; set; } = null;
		public int? Height { get; set; } = null;


		public int ActualWidth { get; protected set; }
		public int ActualHeight { get; protected set; }

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
		

		public virtual void UpdateSize()
		{
			ActualWidth = Width.HasValue ? Width.Value : 0;
			ActualHeight = Height.HasValue ? Height.Value : 0;
		}

		public void UpdateLayout()
		{
			Position = (Container?.ClientBounds.Location ?? Point.Zero) + Offset;
			Bounds = new Rectangle(
				Position, 
				new Point(Size.X + Padding.Left + Padding.Right + Margin.Left + Margin.Right, 
					Size.Y + Padding.Top + Padding.Bottom + Margin.Top + Margin.Bottom)
			);

			ClientBounds = new Rectangle(
				Bounds.X + Padding.Left + Margin.Left,
				Bounds.Y + Padding.Top + Margin.Top,
				Bounds.Width - Padding.Left - Padding.Right - Margin.Left - Margin.Right,
				Bounds.Height - Padding.Top - Padding.Bottom - Margin.Top - Margin.Bottom
			);

			OnUpdateLayout();
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

			// Debug Bounding Boxes
			//DrawDebugBoundingBoxes(renderer);
		}

		protected virtual void OnUpdateLayout()
		{

		}

		protected virtual void OnUpdate(GameTime gameTime)
		{

		}

		private void DrawDebugBoundingBoxes(GuiRenderer renderer)
		{
			renderer.DrawRectangle(ClientBounds, Color.Blue);
			renderer.DrawRectangle(Bounds, Color.Red);
			renderer.DrawRectangle(new Rectangle(Position.X + Margin.Left, Position.Y + Margin.Top, ActualWidth - Margin.Left - Margin.Right, ActualHeight - Margin.Top - Margin.Bottom), Color.Green);
		}

		private void DrawBorder(GuiRenderer renderer)
		{

		}

		private void DrawBackground(GuiRenderer renderer)
		{
			if (BackgroundColor.A > 0.0f)
			{
				renderer.FillRectangle(ClientBounds, BackgroundColor);
			}

			if (BackgroundImage != null)
			{
				renderer.FillRectangle(ClientBounds, BackgroundImage);
			}
		}
	}
}
