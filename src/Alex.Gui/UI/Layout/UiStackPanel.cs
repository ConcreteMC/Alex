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
		
		protected override void OnLayoutControls(UiElementLayoutParameters layoutParameters, IReadOnlyCollection<UiElement> controls)
		{
			var offset = 0;

			foreach (var control in controls.ToArray())
			{
				if (Orientation == Orientation.Horizontal)
				{
					// Increase X
					control.LayoutParameters.Position = new Point(offset, 0);
					offset += control.LayoutParameters.OuterBounds.Width;
				}
				else if (Orientation == Orientation.Vertical)
				{
					// Increase Y
					control.LayoutParameters.Position = new Point(0, offset);
					offset += control.LayoutParameters.OuterBounds.Height;
				}
			}
		}
	}
}
