using System;
using Alex.API.Gui.Elements.Layout;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiSelectionList : GuiStackContainer
    {
        public event EventHandler<GuiSelectionListItem> SelectedItemChanged;

        private GuiSelectionListItem _selectedItem;

        public GuiSelectionListItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;

                SelectedItemChanged?.Invoke(this, _selectedItem);
            }
        }

        public void UnsetSelectedItem(GuiSelectionListItem selected)
        {
            if (SelectedItem == selected)
            {
                SelectedItem = null;
            }
        }

        public void SetSelectedItem(GuiSelectionListItem selected)
        {
            SelectedItem = selected;
        }

        protected override void OnChildAdded(IGuiElement element)
        {
            base.OnChildAdded(element);
            if (element is GuiSelectionListItem listItem)
            {
                listItem.List = this;
            }
        }

        protected override void OnChildRemoved(IGuiElement element)
        {
            base.OnChildRemoved(element);
            if (element is GuiSelectionListItem listItem)
            {
                listItem.List = null;

                if (SelectedItem == listItem)
                {
                    SelectedItem = null;
                }
            }
        }
    }
}
