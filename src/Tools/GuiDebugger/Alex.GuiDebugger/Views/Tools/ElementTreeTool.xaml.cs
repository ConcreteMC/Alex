using System.Linq;
using Alex.GuiDebugger.Models;
using Alex.GuiDebugger.Services;
using Alex.GuiDebugger.ViewModels;
using Alex.GuiDebugger.ViewModels.Documents;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Model.Controls;

namespace Alex.GuiDebugger.Views.Tools
{
    public class ElementTreeTool : UserControl
    {
        public ElementTreeTool()
        {
            this.InitializeComponent();

            this.FindControl<Button>("RefreshButton").Click += async (sender, e) =>
            {
                    var srv   = AlexGuiDebuggerInteraction.Instance;
                    var items = await srv.GetElementTreeItems();

                    if (items != null)
                    {
                        if (DataContext is ViewModels.Tools.ElementTreeTool vm)
                        {
                            vm.ElementTreeItems.Clear();

                            foreach (var item in items)
                            {
                                vm.ElementTreeItems.Add(item);
                            }
                        }
                    }
            };

            this.FindControl<TreeView>("TreeView").SelectionChanged += async (sender, e) =>
            {
                if (!(Application.Current is App app))
                    return;

                if (!(app.MainWindow.DataContext is MainWindowViewModel mainWindowViewModel))
                    return;

                var documentsPane = mainWindowViewModel.Factory.ViewLocator["DocumentsPane"]() as DocumentDock;

                var selectedItem = ((sender as TreeView).SelectedItem as ElementTreeItem);
                if (selectedItem == null) return;

                var existingView =
                    documentsPane.Views?.FirstOrDefault(x => x.Id == $"ElementTreeItem:{selectedItem.Id}");
                if (existingView != null)
                {
                    mainWindowViewModel.Factory.SetFocusedView(documentsPane, existingView);
                }
                else
                {
                    existingView = new ElementTreeDocument(selectedItem)
                    {
                        Id = $"ElementTreeItem:{selectedItem.Id}"
                    };

                    mainWindowViewModel.Factory.AddView(documentsPane, existingView);
                }

            };

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
