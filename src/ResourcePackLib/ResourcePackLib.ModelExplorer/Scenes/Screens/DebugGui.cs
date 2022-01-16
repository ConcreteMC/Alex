using System.Reflection;
using Microsoft.Xna.Framework;
using RocketUI;

namespace ResourcePackLib.ModelExplorer.Scenes.Screens;

public class DebugGui : Screen
{
    private StackContainer _topleft;

    public DebugGui()
    {
        AddChild(_topleft = new StackContainer()
        {
            Anchor = Alignment.TopLeft,
            ChildAnchor = Alignment.TopLeft
        });

        AddDebugLine(Assembly.GetExecutingAssembly().GetName().FullName);
        AddDebugLine($"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
        
        AddDebugLine(() =>
        {
            var p = ModelExplorerGame.Instance.Camera.Position;
            return $"Camera Position: {p.X:F2}, {p.Y:F2}, {p.Z:F2}";
        });
        AddDebugLine(() =>
        {
            var p = ModelExplorerGame.Instance.Camera.Rotation;
            return $"Camera Rotation: {p.X:F2}, {p.Y:F2}, {p.Z:F2}, {p.W:F2}";
        });
        
        AddDebugLine(() =>
        {
            var pos = ModelExplorerGame.Instance.GuiManager.FocusManager.CursorPosition;
            return $"Cursor: {pos.X.ToString("00000")}, {pos.Y.ToString("00000")}";
        });
    }

    private void AddDebugLine(string text)
    {
        _topleft.AddChild(new TextElement(text)
        {
            TextColor = Color.White
        });
    }

    private void AddDebugLine(Func<string> updateFunc)
    {
        _topleft.AddChild(new AutoUpdatingTextElement(updateFunc)
        {
            TextColor = Color.White
        });
    }
}