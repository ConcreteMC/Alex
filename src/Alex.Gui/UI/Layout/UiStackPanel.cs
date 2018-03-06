using System.Linq;
using Alex.Graphics.UI.Common;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Layout
{
	public class UiStackPanel : UiContainer
	{

		public Orientation Orientation { get; set; } = Orientation.Vertical;

		public UiStackPanel()
		{

		}


		public override void UpdateSize()
		{
			base.UpdateSize();

			if (Orientation == Orientation.Horizontal)
			{
				ActualWidth = Controls.Sum(c => c.Bounds.Width);
				ActualHeight = Controls.Max(c => c.Bounds.Height);
			}
			else if (Orientation == Orientation.Vertical)
			{
				ActualWidth = Controls.Max(c => c.Bounds.Width);
				ActualHeight = Controls.Sum(c => c.Bounds.Height);
			}

		}

		protected override void OnUpdateLayout()
		{
			base.OnUpdateLayout();

			var offset = 0;

			foreach (var control in Controls.ToArray())
			{
				if (Orientation == Orientation.Horizontal)
				{
					// Increase X
					control.Offset = new Point(offset, control.Offset.Y);
					offset += control.OuterBounds.Width;
				}
				else if (Orientation == Orientation.Vertical)
				{
					// Increase Y
					control.Offset = new Point(control.Offset.X, offset);
					offset += control.OuterBounds.Height;
				}

				control.UpdateLayout();
			}
		}
	}
}
