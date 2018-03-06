using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI
{
	public class UiContainer : UiElement
	{
		public ObservableCollection<UiElement> Controls { get; } = new ObservableCollection<UiElement>();


		public VerticalAlignment VerticalContentAlignment => Style.VerticalContentAlignment;
		public HorizontalAlignment HorizontalContentAlignment => Style.HorizontalContentAlignment;


		public UiContainer(int? width, int? height) : base(width, height)
		{
			Controls.CollectionChanged += ControlsOnCollectionChanged;
		}

		public UiContainer() : this(null, null)
		{
		}

		private void ControlsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			//if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace ||
			//e.Action == NotifyCollectionChangedAction.Move)
			if (e.OldItems != null && e.OldItems.Count > 0)
			{
				foreach (UiElement eOldItem in e.OldItems)
				{
					eOldItem.Container = null;
					eOldItem.Offset = Point.Zero;
					eOldItem.LayoutChanged -= OnChildControlLayoutChanged;
				}
			}

			if (e.NewItems != null && e.NewItems.Count > 0)
			{
				foreach (UiElement eNewItem in e.NewItems)
				{
					eNewItem.Container = this;
					eNewItem.Offset = Point.Zero;
					eNewItem.UpdateSize();
					eNewItem.LayoutChanged += OnChildControlLayoutChanged;
				}
			}

			MarkLayoutDirty();
		}

		private void OnChildControlLayoutChanged(object sender, EventArgs eventArgs)
		{
			UpdateSize();
			MarkLayoutDirty();
		}

		public override void UpdateSize()
		{
			var controls = Controls.ToArray();
			if (!controls.Any())
			{
				base.UpdateSize();
				return;
			}
			
			foreach (var control in controls)
			{
				control.UpdateSize();
			}

			var childWidth = controls.Max(c => c.OuterBounds.Right) - controls.Min(c => c.OuterBounds.Left);
			var childHeight = controls.Max(c => c.OuterBounds.Bottom) - controls.Min(c => c.OuterBounds.Top);

			if (Style.WidthSizeMode == SizeMode.Absolute)
			{
				ActualWidth = Style.Width ?? 0;
			}
			else if (Style.WidthSizeMode == SizeMode.FillParent)
			{
				ActualWidth = Container?.ClientBounds.Width ?? 0;
			}
			else if (Style.WidthSizeMode == SizeMode.Auto)
			{
				ActualWidth = Style.Width ?? childWidth;
			}

			if (Style.HeightSizeMode == SizeMode.Absolute)
			{
				ActualHeight = Style.Height ?? 0;
			}
			else if (Style.HeightSizeMode == SizeMode.FillParent)
			{
				ActualHeight = Container?.ClientBounds.Height ?? 0;
			}
			else if (Style.HeightSizeMode == SizeMode.Auto)
			{
				ActualHeight = Style.Height ?? childHeight;
			}


			UpdateBounds();

			var fillWidthControls = controls.Where(c => c.Style.WidthSizeMode == SizeMode.FillParent).ToArray();
			var fillHeightControls = controls.Where(c => c.Style.HeightSizeMode == SizeMode.FillParent).ToArray();

			foreach (var c in fillWidthControls)
			{
				c.ActualWidth = ClientBounds.Width - c.Style.Margin.Horizontal;
			}

			foreach (var c in fillHeightControls)
			{
				c.ActualHeight = ClientBounds.Height - c.Style.Margin.Vertical;
			}
		}

		protected internal override Point GetAutoSize()
		{
			var controls = Controls.ToArray();

			var baseSize = base.GetAutoSize();

			if (!controls.Any())
			{
				return baseSize;
			}

			var maxWidth = 0;
			var maxHeight = 0;

			foreach (var control in controls)
			{
				var s = control.GetAutoSize();

				maxWidth = Math.Max(maxWidth, s.X);
				maxHeight = Math.Max(maxHeight, s.Y);
			}

			// Width
			SizeMode widthMode = Style.WidthSizeMode;
			//ActualWidth = maxWidth;

			// Height
			SizeMode heightMode = Style.HeightSizeMode;
			//ActualHeight = maxHeight;

			return new Point(Math.Max(baseSize.X, maxWidth), Math.Max(baseSize.Y, maxHeight));
		}

		protected override void OnDraw(GameTime gameTime, UiRenderer renderer)
		{
			base.OnDraw(gameTime, renderer);

			foreach (var control in Controls.ToArray())
			{
				if (control.Visible)
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

		protected override void OnUpdateLayout()
		{
			base.OnUpdateLayout();

			if (!Controls.Any()) return;

			var controls = Controls.ToArray();

			foreach (var control in controls)
			{
				int offsetX = control.Offset.X, offsetY = control.Offset.Y;

				if (HorizontalContentAlignment == HorizontalAlignment.Right)
				{
					offsetX = ClientBounds.Width - control.OuterBounds.Width;
				}
				else if (HorizontalContentAlignment == HorizontalAlignment.Center)
				{
					offsetX = (int)Math.Floor((ClientBounds.Width - control.OuterBounds.Width) / 2f);
				}
				else if (HorizontalContentAlignment == HorizontalAlignment.Left)
				{
					offsetX = 0;
				}

				if (VerticalContentAlignment == VerticalAlignment.Bottom)
				{
					offsetY = ClientBounds.Height - control.OuterBounds.Height;
				}
				else if (VerticalContentAlignment == VerticalAlignment.Center)
				{
					offsetY = (int)Math.Floor((ClientBounds.Height - control.OuterBounds.Height) / 2f);
				}
				else if (VerticalContentAlignment == VerticalAlignment.Top)
				{
					offsetY = 0;
				}

				control.Offset = new Point(offsetX, offsetY);

				control.UpdateLayout();
			}

		}
	}
}
