using Avalonia;
using Avalonia.Markup.Xaml;

namespace Alex.Launcher
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
   }
}