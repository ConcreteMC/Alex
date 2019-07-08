using System;
using Alex.GuiDebugger.Services;
using Alex.GuiDebugger.ViewModels;
using Avalonia;
using Alex.GuiDebugger.ViewModels.Tools;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
