using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Alex.GuiDebugger.Models;

namespace Alex.GuiDebugger.ViewModels
{
    public class ElementTreeViewModel : ViewModelBase
    {
        public ObservableCollection<ElementTreeItem> ElementTreeItems { get; set; }

        public ElementTreeViewModel(IEnumerable<ElementTreeItem> elementTreeItems)
        {
            ElementTreeItems = new ObservableCollection<ElementTreeItem>(elementTreeItems);
        }

    }
}
