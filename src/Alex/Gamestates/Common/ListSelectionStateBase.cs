using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Events;
using RocketUI.Input;

namespace Alex.Gamestates.Common
{
	public class ListItem : SelectionListItem
	{
		public EventHandler CursorDoubleClick;

		protected ListItem() { }

		private DateTime _lastClick = DateTime.MinValue;

		/// <inheritdoc />
		protected override void OnCursorPressed(Point cursorPosition, MouseButton button)
		{
			base.OnCursorPressed(cursorPosition, button);

			if (!Focused)
				return;

			if (button == MouseButton.Left)
			{
				var currentTime = DateTime.UtcNow;
				var timeSpent = currentTime - _lastClick;

				if (timeSpent.TotalMilliseconds < 500)
				{
					CursorDoubleClick?.Invoke(this, EventArgs.Empty);
				}

				_lastClick = currentTime;
			}
		}
	}

	public class ListSelectionStateBase<TGuiListItemContainer> : GuiMenuStateBase where TGuiListItemContainer : ListItem
	{
		public override int BodyMinWidth => 356;

		protected TGuiListItemContainer[] Items => _items.ToArray();
		private List<TGuiListItemContainer> _items { get; } = new List<TGuiListItemContainer>();

		private TGuiListItemContainer _selectedItem;

		public TGuiListItemContainer SelectedItem
		{
			get => _selectedItem;
			set
			{
				if (_selectedItem == value) return;
				_selectedItem = value;

				OnSelectedItemChanged(value);
			}
		}

		public ListSelectionStateBase() : base()
		{
			Body.BackgroundOverlay = new Color(Color.Black, 0.35f);
		}

		public TGuiListItemContainer this[int index]
		{
			get
			{
				return _items[index];
			}
			set
			{
				_items[index] = value;
			}
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
			GuiManager.FocusManager.FocusChanged += OnFocusChanged;
		}

		private void OnFocusChanged(object sender, GuiFocusChangedEventArgs e)
		{
			if (e.FocusedElement == null || !(e.FocusedElement is TGuiListItemContainer listItem))
				return;

			if (!_items.Contains(listItem))
				return;

			SetSelectedItem(listItem);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
		}

		private void CursorDoubleClick(object sender, EventArgs e)
		{
			if (sender is not TGuiListItemContainer item)
				return;

			OnItemDoubleClick(item);
		}

		protected virtual void OnItemDoubleClick(TGuiListItemContainer item) { }

		public void AddItem(TGuiListItemContainer item)
		{
			item.CursorDoubleClick += CursorDoubleClick;

			_items.Add(item);
			Body.AddChild(item);

			OnAddItem(item);
		}

		protected virtual void OnAddItem(TGuiListItemContainer item) { }

		public void RemoveItem(TGuiListItemContainer item)
		{
			item.CursorDoubleClick -= CursorDoubleClick;

			Body.RemoveChild(item);
			_items.Remove(item);

			if (SelectedItem == item)
				SelectedItem = null;

			OnRemoveItem(item);
		}

		protected virtual void OnRemoveItem(TGuiListItemContainer item) { }

		public void UnsetSelectedItem(TGuiListItemContainer selected)
		{
			if (SelectedItem == selected)
			{
				SelectedItem = null;
			}
		}

		public void SetSelectedItem(TGuiListItemContainer selected)
		{
			SelectedItem = selected;
		}

		protected virtual void OnSelectedItemChanged(TGuiListItemContainer newItem) { }

		public void ClearItems()
		{
			SelectedItem = null;

			foreach (var item in _items)
			{
				Body.RemoveChild(item);
			}

			_items.Clear();
		}
	}
}