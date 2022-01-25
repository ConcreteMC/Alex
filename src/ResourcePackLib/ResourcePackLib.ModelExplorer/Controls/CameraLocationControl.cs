using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using ResourcePackLib.ModelExplorer.Abstractions;
using RocketUI;

namespace ResourcePackLib.ModelExplorer.Controls;

public class CameraLocationControl : RocketControl
{
    private AutoUpdatingTextInput _posX;
    private AutoUpdatingTextInput _posY;
    private AutoUpdatingTextInput _posZ;

    public CameraLocationControl()
    {
        var stack = new MultiStackContainer()
        {
            Anchor = Alignment.Fill,
            ChildAnchor = Alignment.Fill,
            Orientation = Orientation.Vertical
        };

        stack.AddRow(
            _posX = new AutoUpdatingTextInput(() => (GuiManager.Game as IGame)?.ServiceProvider.GetRequiredService<ICamera>().Position.X.ToString("F2")),
            _posY = new AutoUpdatingTextInput(() => (GuiManager.Game as IGame)?.ServiceProvider.GetRequiredService<ICamera>().Position.Y.ToString("F2")),
            _posZ = new AutoUpdatingTextInput(() => (GuiManager.Game as IGame)?.ServiceProvider.GetRequiredService<ICamera>().Position.Z.ToString("F2"))
        );
        AddChild(stack);
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        base.OnUpdate(gameTime);
    }
}