using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Alex.GuiDebugger.Views
{
    public class DockWindow : Window
    {
        public DockWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
