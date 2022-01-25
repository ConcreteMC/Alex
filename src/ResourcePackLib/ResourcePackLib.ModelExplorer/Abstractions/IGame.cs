using Microsoft.Xna.Framework;
using ResourcePackLib.ModelExplorer.Scenes;
using RocketUI;
using RocketUI.Input;

namespace ResourcePackLib.ModelExplorer.Abstractions;

public interface IGame : IDisposable
{
    Game Game { get; }
        
    GraphicsDeviceManager DeviceManager { get; }
        
    // include all the Properties/Methods that you'd want to use on your Game class below.
    GameWindow                   Window       { get; }
    ICamera                      Camera       { get; }
    SceneManager                 SceneManager { get; }
    InputManager                 InputManager { get; }
    
    IServiceProvider ServiceProvider { get; }
    GuiManager           GuiManager      { get; }

    event EventHandler<EventArgs> Exiting;

    void Run();
    void Exit();
}