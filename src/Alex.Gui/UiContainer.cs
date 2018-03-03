using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Alex.Gui.Enums;
using Alex.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui
{
	public class UiContainer : UiElement
	{
		public ObservableCollection<UiElement> Controls { get; }


		public VerticalAlignment VerticalContentAlignment { get; set; } = VerticalAlignment.None;
		public HorizontalAlignment HorizontalContentAlignment { get; set; } = HorizontalAlignment.None;


		public UiContainer(int? width, int? height) : base(width, height)
		{
			Controls = new ObservableCollection<UiElement>();
			Controls.CollectionChanged += ControlsOnCollectionChanged;
		}

		private void ControlsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (UiElement eNewItem in e.NewItems)
				{
					eNewItem.Container = this;
				}
			}

			UpdateSize();
			Layout();
		}

		public UiContainer() : this(null, null)
		{
		}

		protected void Layout()
		{
			OnLayout();
		}

		public override void UpdateSize()
		{
			var controls = Controls.ToArray();
			if (!controls.Any())
			{
				ActualWidth = Width.HasValue ? Width.Value : 0;
				ActualHeight = Height.HasValue ? Height.Value : 0;
				return;
			}

			foreach (var control in controls)
			{
				control.UpdateSize();
			}

			var childWidth = controls.Max(c => c.Bounds.Right) - controls.Min(c => c.Bounds.Left);
			var childHeight = controls.Max(c => c.Bounds.Bottom) - controls.Min(c => c.Bounds.Top);

			ActualWidth = Math.Max(Width.HasValue ? Width.Value : 0, childWidth);
			ActualHeight = Math.Max(Height.HasValue ? Height.Value : 0, childHeight);
		}

		protected virtual void OnLayout() { }
		
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

		protected override void OnUpdateLayout()
		{
			base.OnUpdateLayout();

			foreach (var control in Controls.ToArray())
			{
				int offsetX = 0, offsetY = 0;

				if (HorizontalContentAlignment == HorizontalAlignment.Right)
				{
					offsetX = ClientBounds.Width - control.Bounds.Width;
				}
				else if (HorizontalContentAlignment == HorizontalAlignment.Center)
				{
					offsetX = (int)Math.Floor((ClientBounds.Width - control.Bounds.Width) / 2f);
				}

				if (VerticalContentAlignment == VerticalAlignment.Bottom)
				{
					offsetY = ClientBounds.Height - control.Bounds.Height;
				}
				else if (VerticalContentAlignment == VerticalAlignment.Center)
				{
					offsetY = (int) Math.Floor((ClientBounds.Height - control.Bounds.Height) / 2f);
				}

				control.Offset = new Point(offsetX, offsetY);
				
				control.UpdateLayout();
			}

		}
	}
}
