using Avalonia;
using Avalonia.Markup.Xaml;

namespace Alex.GuiDebugger
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
   }
}