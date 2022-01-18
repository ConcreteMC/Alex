using RocketUI;
using RocketUI.Serialization.Xaml;

namespace ResourcePackLib.ModelExplorer.Scenes.Screens;

public partial class MainMenuScreen : Screen
{

    public MainMenuScreen() : base()
    {
        RocketXamlLoader.Load(this);
    }
}