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
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
