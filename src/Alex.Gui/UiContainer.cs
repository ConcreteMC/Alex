using System;
using System.Collections.Generic;
using Alex.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui
{
	public class UiContainer : UiElement
	{
		public List<UiElement> Controls { get; }

		public UiContainer(int? width, int? height) : base(width, height)
		{
			Controls = new List<UiElement>();

		}

		public UiContainer() : this(null, null)
		{
		}

		protected void Layout()
		{
			OnLayout();
		}

		protected virtual void OnLayout() { }

		protected internal override void OnApplySkin(UiSkin skin)
		{
			base.OnApplySkin(skin);

			foreach (var control in Controls.ToArray())
			{
				control.ApplySkin(skin);
			}
		}

		protected override void OnDraw(GameTime gameTime, GuiRenderer renderer)
		{
			base.OnDraw(gameTime, renderer);

			foreach (var control in Controls.ToArray())
			{
				control.Draw(gameTime, renderer);
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			foreach (var control in Controls.ToArray())
			{
				control.Update(gameTime);
			}
		}
	}
}
