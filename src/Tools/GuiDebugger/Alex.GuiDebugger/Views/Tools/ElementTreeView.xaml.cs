using Alex.GuiDebugger.ViewModels;
using Avalonia;
using Alex.GuiDebugger.ViewModels.Tools;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Alex.GuiDebugger.Views.Tools
{
    public class ElementTreeView : UserControl
    {
        public ElementTreeView()
        {
            this.InitializeComponent();
            DataContext = new ElementTreeToolViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
