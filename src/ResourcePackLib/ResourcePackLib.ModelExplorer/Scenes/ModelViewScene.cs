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

        Components.Add(new GridEntity(ModelExplorerGame.Instance)
        {
            Visible = true,
            Scale = Vector3.One,
            Position = Vector3.Zero,
            Steps = 25,
            SubDivisions = 16,
            Spacing = 1.0f,
            LineColor = Color.LightGray * 0.85f,
            SubLineColor = Color.LightGray * 0.15f
        });
        
        var cube = new CubeEntity(ModelExplorerGame.Instance);
        cube.Visible = true;
        cube.Position = Vector3.Zero;
        cube.Scale = Vector3.One;

        var fox = Services.CreateInstance<MCEntity>();
        fox.Visible = true;
        fox.Position = Vector3.Zero;
        fox.Transform.Scale = Vector3.One / 16f;
        
        Components.Add(Services.GetOrCreateInstance<AxisEntity>());
        //Components.Add(cube);
        Components.Add(fox);
        
        Components.Add(Services.GetOrCreateInstance<CameraMouseController>());
        Components.Add(Services.GetOrCreateInstance<CameraKeyboardController>());
        
        Game.Camera.Position = Vector3.Backward;
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        var a = "b";
        foreach (var c in Components)
        {
            if (c is MCEntity mcEntity)
            {
               // mcEntity.Scale = Vector3.One / 16f;
            }
        }
        base.OnUpdate(gameTime);
    }

    protected override void OnShow()
    {
        base.OnShow();
    }
}