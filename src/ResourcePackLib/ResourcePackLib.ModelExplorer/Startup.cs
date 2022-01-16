using Microsoft.Extensions.DependencyInjection;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Graphics;
using ResourcePackLib.ModelExplorer.Scenes;
using RocketUI;
using RocketUI.Debugger;
using RocketUI.Input;
using RocketUI.Input.Listeners;
using RocketUI.Utilities.Extensions;

namespace ResourcePackLib.ModelExplorer;

public static class Startup
{

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddInputListenerFactory(p => new MouseInputListener(p));
        services.AddInputListenerFactory(p => new KeyboardInputListener(p));
        services.AddSingleton<SceneManager>();
        services.AddSingleton<ICamera>(sp => sp.GetRequiredService<IGame>().Camera);
        services.AddSingleton<InputManager>();
        services.AddSingleton<IGuiRenderer, GuiRenderer>();
        services.AddSingleton<GuiManager>();
        services.AddSingleton<RocketDebugSocketServer>();
        services.AddHostedService<RocketDebugSocketServer>(sp => sp.GetRequiredService<RocketDebugSocketServer>());

    }
    
}