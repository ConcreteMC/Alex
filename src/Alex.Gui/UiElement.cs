using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Common;
using Alex.Gui.Rendering;
using Alex.Gui.Themes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui
{
	public class UiElement
	{
		public UiContainer Container { get; internal set; }
		
		public Rectangle OuterBounds { get; private set; }
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

		public string ClassName { get; set; }

		#region Styling Properties

		public UiElementStyle Style { get; private set; } = new UiElementStyle();
		
		public Thickness Margin { get; set; } = Thickness.Zero;
		public Thickness Padding { get; set; } = Thickness.Zero;
		
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

			UpdateLayout();
		}

		public void UpdateLayout()
		{
			Position = (Container?.ClientBounds.Location ?? Point.Zero) + Offset;
			OuterBounds = new Rectangle(
				Position, 
				new Point(Size.X + Padding.Left + Padding.Right + Margin.Left + Margin.Right, 
					Size.Y + Padding.Top + Padding.Bottom + Margin.Top + Margin.Bottom)
			);

			Bounds = new Rectangle(
				OuterBounds.X + Margin.Left,
				OuterBounds.Y + Margin.Top,
				OuterBounds.Width - Margin.Left - Margin.Right,
				OuterBounds.Height - Margin.Top - Margin.Bottom
			);

			ClientBounds = new Rectangle(
				OuterBounds.X + Padding.Left + Margin.Left,
				OuterBounds.Y + Padding.Top + Margin.Top,
				OuterBounds.Width - Padding.Left - Padding.Right - Margin.Left - Margin.Right,
				OuterBounds.Height - Padding.Top - Padding.Bottom - Margin.Top - Margin.Bottom
			);

			OnUpdateLayout();
		}

		public void Draw(GameTime gameTime, GuiRenderer renderer)
		{
			Style = renderer.Theme.GetCompiledStyleFor(this);

			OnDraw(gameTime, renderer);
		}

		public void Update(GameTime gameTime)
		{
			OnUpdate(gameTime);
		}

		protected virtual void OnDraw(GameTime gameTime, GuiRenderer renderer)
		{
			renderer.DrawElement(this);
		}

		protected virtual void OnUpdateLayout()
		{

		}

		protected virtual void OnUpdate(GameTime gameTime)
		{

		}
	}
}
