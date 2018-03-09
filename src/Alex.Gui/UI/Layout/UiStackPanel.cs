using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Graphics.UI.Common;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Layout
{
	public class UiStackPanel : UiContainer
	{
		private Orientation _orientation = Orientation.Vertical;

		public Orientation Orientation
		{
			get => _orientation;
			set
			{
				if (value == _orientation) return;
				_orientation = value;
				OnPropertyChanged();
			}
		}

		public UiStackPanel()
		{
		}

		protected override Vector2 GetContentSize()
		{
			if (Orientation == Orientation.Horizontal)
			{
				var controls = Controls.ToArray();

				var width = controls.Sum(c => c.LayoutParameters.OuterBounds.Width);
				var maxHeight   = controls.Max(c => c.LayoutParameters.OuterBounds.Height);

				return new Vector2(width, maxHeight);
			}
			else if (Orientation == Orientation.Vertical)
			{
				var controls = Controls.ToArray();

				var maxWidth = controls.Max(c => c.LayoutParameters.OuterBounds.Width);
				var height   = controls.Sum(c => c.LayoutParameters.OuterBounds.Height);
				
				return new Vector2(maxWidth, height);
			}

			return base.GetContentSize();
		}

		protected override void OnLayoutControls(UiElementLayoutParameters layout,
			IReadOnlyCollection<UiElement>                                 controls)
		{
			base.OnLayoutControls(layout, controls);

			var offset = 0;
			foreach (var control in controls.ToArray())
			{
				if (Orientation == Orientation.Horizontal)
				{
					// Increase X
					control.LayoutParameters.RelativePosition =  new Point(offset, control.LayoutParameters.RelativePosition.Y);
					offset                                    += control.LayoutParameters.OuterBounds.Width;
					
				}
				else if (Orientation == Orientation.Vertical)
				{
					// Increase Y
					control.LayoutParameters.RelativePosition =  new Point(control.LayoutParameters.RelativePosition.X, offset);
					offset                                    += control.LayoutParameters.OuterBounds.Height;
					
				}
			}
		}
	}
}