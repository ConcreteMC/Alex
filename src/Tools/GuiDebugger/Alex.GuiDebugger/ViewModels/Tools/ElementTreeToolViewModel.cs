using System.Collections.Generic;
using System.Collections.ObjectModel;
using Alex.GuiDebugger.Models;
using Dock.Model.Controls;

namespace Alex.GuiDebugger.ViewModels.Tools
{
    public class ElementTreeToolViewModel : ToolTab
    {
        public ObservableCollection<ElementTreeItem> ElementTreeItems { get; set; }

        public ElementTreeToolViewModel(IEnumerable<ElementTreeItem> elementTreeItems)
        {
            ElementTreeItems = new ObservableCollection<ElementTreeItem>(elementTreeItems);
        }

    }
}
