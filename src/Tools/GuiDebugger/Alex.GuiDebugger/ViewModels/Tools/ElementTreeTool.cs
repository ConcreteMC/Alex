using System.Collections.Generic;
using System.Collections.ObjectModel;
using Alex.GuiDebugger.Models;
using Alex.GuiDebugger.Services;
using Dock.Model.Controls;
using DynamicData;
using ReactiveUI;

namespace Alex.GuiDebugger.ViewModels.Tools
{
    public class ElementTreeTool : ToolTab
    {
        public ObservableCollection<ElementTreeItem> ElementTreeItems { get; }

        public ReactiveCommand RefreshCommand { get; }

        public ElementTreeTool()
        {
            ElementTreeItems = new ObservableCollection<ElementTreeItem>();
            RefreshCommand = ReactiveCommand.Create(Refresh);
            //Refresh();
        }


        private void Refresh()
        {
            var newItems = AlexGuiDebuggerInteraction.Instance.GetElementTreeItems().Result;
            ElementTreeItems.Clear();
            ElementTreeItems.AddRange(newItems);
        }
    }
}
