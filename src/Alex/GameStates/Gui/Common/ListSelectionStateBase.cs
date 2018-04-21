using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.GameStates.Gui.Common
{
    public class ListSelectionStateBase<TGuiListItemContainer> : GuiStateBase where TGuiListItemContainer : GuiSelectionListItem
    {

        protected TGuiListItemContainer[] Items => _items.ToArray();
        private List<TGuiListItemContainer> _items { get; } = new List<TGuiListItemContainer>();

	    protected TGuiListItemContainer SelectedItem => ListContainer.SelectedItem as TGuiListItemContainer;
        protected readonly GuiSelectionList ListContainer;

        public ListSelectionStateBase() : base()
        {
	        AddGuiElement(ListContainer = new GuiSelectionList()
            {
				BackgroundOverlayColor = new Color(Color.Black, 0.65f),
                //Y = Header.Height,
                //Width = 322,
	            Anchor = Alignment.Fill,
				ChildAnchor = Alignment.TopCenter,
            });
	        ListContainer.SelectedItemChanged += HandleSelectedItemChanged;
			ListContainer.Margin = new Thickness(0, Header.Height, 0, Footer.Height);
        }

	    protected override void OnUpdate(GameTime gameTime)
	    {
		    base.OnUpdate(gameTime);
	    }

	    public void AddItem(TGuiListItemContainer item)
        {
            _items.Add(item);
            ListContainer.AddChild(item);
        }
        
        public void RemoveItem(TGuiListItemContainer item)
        {
            ListContainer.RemoveChild(item);
            _items.Remove(item);
        }

	    private void HandleSelectedItemChanged(object sender, GuiSelectionListItem item)
	    {
			OnSelectedItemChanged(item as TGuiListItemContainer);
	    }

	    protected virtual void OnSelectedItemChanged(TGuiListItemContainer newItem)
	    {

	    }

	    public void ClearItems()
	    {
		    foreach (var item in _items)
		    {
			    ListContainer.RemoveChild(item);
		    }
			_items.Clear();
	    }
    }
}
