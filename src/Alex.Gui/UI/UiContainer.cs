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


			var minX = controls.Min(c => c.LayoutParameters.OuterBounds.Left);
			var minY = controls.Min(c => c.LayoutParameters.OuterBounds.Top);
			var maxX = controls.Max(c => c.LayoutParameters.OuterBounds.Right);
			var maxY = controls.Max(c => c.LayoutParameters.OuterBounds.Bottom);

			return new Vector2(Math.Abs(maxX - minX), Math.Abs(maxY - minY));
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

			var controls = Controls.ToArray();
			if (!controls.Any()) return;

			foreach (var control in controls)
			{
				if (control == null) continue;
				
				control.Update(gameTime);
			}
		}

		protected override void OnUpdateLayout(UiElementLayoutParameters layout)
		{
			base.OnUpdateLayout(layout);

			var controls = Controls;
			if (controls.Any())
			{
				foreach (var control in controls)
				{
					control.LayoutParameters.BasePosition = layout.InnerBounds.Location;
					control.UpdateLayoutInternal();
				}
			}

			layout.ContentSize = GetContentSize();
		}

		protected override void OnPostUpdateLayout(UiElementLayoutParameters layout)
		{
			base.OnPostUpdateLayout(layout);

			var controls = Controls;
			if (controls.Any())
			{
				OnLayoutControls(layout, controls);
			}
		}

		protected virtual void OnLayoutControls(UiElementLayoutParameters layout,
			IReadOnlyCollection<UiElement>                                controls)
		{
			foreach (var control in controls)
			{
				var cLayout = control.LayoutParameters;
				
				if (cLayout.SizeAnchor.HasValue)
				{
					var newSize = layout.InnerBounds.Size.ToVector2() * cLayout.SizeAnchor.Value;

					cLayout.AutoSize = new Vector2(cLayout.SizeAnchor.Value.X < 0 ? cLayout.Size.X : newSize.X, cLayout.SizeAnchor.Value.Y < 0 ? cLayout.Size.Y : newSize.Y);
				}

				if (cLayout.PositionAnchor.HasValue)
				{
					cLayout.RelativePosition = (
						(layout.InnerBounds.Size.ToVector2() * cLayout.PositionAnchor.Value) + cLayout.InnerBounds.Size.ToVector2() * (cLayout.PositionAnchorOrigin)
						- (cLayout.Size * cLayout.SizeAnchorOrigin)
					).ToPoint();
				}
			}
		}
	}
}