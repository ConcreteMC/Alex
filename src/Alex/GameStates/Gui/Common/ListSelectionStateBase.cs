using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui;

namespace Alex.GameStates.Gui.Common
{
    public class ListSelectionStateBase<TGuiListItemContainer> : GuiStateBase where TGuiListItemContainer : GuiContainer
    {
        protected TGuiListItemContainer[] Items => _items.ToArray();
        private List<TGuiListItemContainer> _items { get; } = new List<TGuiListItemContainer>();

        public ListSelectionStateBase(Alex alex) : base(alex)
        {
        }

        public void AddItem(TGuiListItemContainer item)
        {
            _items.Add(item);
        }
        
        public void RemoveItem(TGuiListItemContainer item)
        {
            _items.Remove(item);
        }
    }
}
