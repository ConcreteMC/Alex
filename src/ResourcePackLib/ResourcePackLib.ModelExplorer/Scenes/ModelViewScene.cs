using Microsoft.Xna.Framework;
using ResourcePackLib.ModelExplorer.Components;
using ResourcePackLib.ModelExplorer.Entities;
using ResourcePackLib.ModelExplorer.Scenes.Screens;
using ResourcePackLib.ModelExplorer.Utilities;

namespace ResourcePackLib.ModelExplorer.Scenes;

public class MainMenuScene : GuiSceneBase<MainMenuScreen>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        
        var cube = new CubeEntity(ModelExplorerGame.Instance);
        cube.Visible = true;
        cube.Position = Vector3.Zero;
        cube.Scale = Vector3.One;
        
        Components.Add(Services.GetOrCreateInstance<AxisEntity>());
        Components.Add(cube);
        
        Components.Add(Services.GetOrCreateInstance<CameraMouseController>());
    }
}