using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Events;

namespace Alex.Gamestates.Common
{
    public class ListSelectionStateBase<TGuiListItemContainer> : GuiMenuStateBase 
		where TGuiListItemContainer : SelectionListItem
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
		      //  SelectedItemChanged?.Invoke(this, _selectedItem);
	        }
        }
        public ListSelectionStateBase() : base()
        {
	        Body.BackgroundOverlay = new Color(Color.Black, 0.35f);

	       /* AddRocketElement(ListContainer = new SelectionList()
            {
	            Anchor = Alignment.Fill,
				ChildAnchor = Alignment.TopFill,
            });
	        SelectedItemChanged += HandleSelectedItemChanged;*/
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

        private void OnFocusChanged(object? sender, GuiFocusChangedEventArgs e)
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

	    public void AddItem(TGuiListItemContainer item)
        {
            _items.Add(item);
            Body.AddChild(item);
        }
        
        public void RemoveItem(TGuiListItemContainer item)
        {
            Body.RemoveChild(item);
            _items.Remove(item);

            if (SelectedItem == item)
	            SelectedItem = null;
        }
        
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

        protected virtual void OnSelectedItemChanged(TGuiListItemContainer newItem)
	    {

	    }

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
