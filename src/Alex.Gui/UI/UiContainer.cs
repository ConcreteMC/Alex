using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Layout;
using Alex.Graphics.UI.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI
{
	public class UiContainer : UiElement
	{
		private readonly List<UiElement> _controls = new List<UiElement>();

		public IReadOnlyCollection<UiElement> Controls => _controls.ToList();

		
		public UiContainer() : base()
		{
		}
		
		public void AddChild(UiElement element)
		{
			if (element.Container != null)
			{
				element.Container.RemoveChild(element);
			}
			
			element.Container = this;
			_controls.Add(element);

			MarkLayoutDirty();
		}

		public void RemoveChild(UiElement element)
		{
			if (element.Container == this)
			{
				element.Container = null;
			}

			_controls.Remove(element);

			MarkLayoutDirty();
		}

		protected override Vector2 GetContentSize()
		{
			var controls = Controls;
			if (!controls.Any()) return base.GetContentSize();


			var minX = controls.Min(c => c.LayoutParameters.OuterBounds.X);
			var minY = controls.Min(c => c.LayoutParameters.OuterBounds.Y);
			var maxX = controls.Max(c => c.LayoutParameters.OuterBounds.X + c.LayoutParameters.OuterBounds.Width);
			var maxY = controls.Max(c => c.LayoutParameters.OuterBounds.Y + c.LayoutParameters.OuterBounds.Height);

			return new Vector2(maxX - minX, maxY - minY);

		}

		protected override void OnDraw(GameTime gameTime, UiRenderer renderer)
		{
			base.OnDraw(gameTime, renderer);

			var controls = Controls;
			if (!controls.Any()) return;

			foreach (var control in controls)
			{
				if (control.Visible)
					control.Draw(gameTime, renderer);
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			var controls = Controls;
			if (!controls.Any()) return;

			foreach (var control in controls)
			{
				control.Update(gameTime);
			}
		}

		protected override void OnUpdateLayout(UiElementLayoutParameters layoutParameters)
		{
			base.OnUpdateLayout(layoutParameters);

			var controls = Controls;
			if (!controls.Any()) return;

			foreach (var control in controls)
			{
				control.UpdateLayoutInternal();
			}

			OnLayoutControls(layoutParameters, controls);
		}

		protected virtual void OnLayoutControls(UiElementLayoutParameters layoutParameters,
			IReadOnlyCollection<UiElement>                                controls)
		{

		}
	}
}
