using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Alex.GuiDebugger.Models;
using Alex.GuiDebugger.Services;
using Dock.Model.Controls;
using DynamicData;

namespace Alex.GuiDebugger.ViewModels
{
    public class ElementTreeToolViewModel : ToolTab
    {
        public ObservableCollection<ElementTreeItem> ElementTreeItems { get; set; }

        private AlexGuiDebuggerInteraction _alexGuiDebuggerInteraction = AlexGuiDebuggerInteraction.Instance;

        public ElementTreeToolViewModel()
        {
            ElementTreeItems = new ObservableCollection<ElementTreeItem>();
            Refresh();
        }

        public void Refresh()
        {
            var items = _alexGuiDebuggerInteraction.GetElementTreeItems().Result;
            ElementTreeItems.Clear();
            ElementTreeItems.AddRange(items);
        }

    }
}
