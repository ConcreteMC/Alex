using System;
using RocketUI.Elements.Layout;

namespace RocketUI.Elements.Controls
{
    public class SelectionList : GuiStackContainer
    {
        public event EventHandler<SelectionListItem> SelectedItemChanged;

        private SelectionListItem _selectedItem;

        public SelectionListItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;

                SelectedItemChanged?.Invoke(this, _selectedItem);
            }
        }

        public void UnsetSelectedItem(SelectionListItem selected)
        {
            if (SelectedItem == selected)
            {
                SelectedItem = null;
            }
        }

        public void SetSelectedItem(SelectionListItem selected)
        {
            SelectedItem = selected;
        }

        protected override void OnChildAdded(IVisualElement element)
        {
            base.OnChildAdded(element);
            if (element is SelectionListItem listItem)
            {
                listItem.List = this;
            }
        }

        protected override void OnChildRemoved(IVisualElement element)
        {
            base.OnChildRemoved(element);
            if (element is SelectionListItem listItem)
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
