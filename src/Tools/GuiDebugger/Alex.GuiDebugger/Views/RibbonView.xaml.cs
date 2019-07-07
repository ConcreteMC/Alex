using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Alex.GuiDebugger.Views
{
    public class RibbonView : UserControl
    {
        public RibbonView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
