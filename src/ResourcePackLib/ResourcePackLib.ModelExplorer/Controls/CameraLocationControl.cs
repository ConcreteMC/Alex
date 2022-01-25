using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Components;
using ResourcePackLib.ModelExplorer.Utilities;
using RocketUI;
using Vector3 = System.Numerics.Vector3;

namespace ResourcePackLib.ModelExplorer.Controls;

public class CameraLocationControl : RocketControl
{
    private AutoUpdatingTextInput _posX;
    private AutoUpdatingTextInput _posY;
    private AutoUpdatingTextInput _posZ;
    private AutoUpdatingTextInput _movementSpeed;
    private AutoUpdatingTextInput _rotationSpeed;

    private ICamera _camera;
    private CameraKeyboardController _cameraController;

    public CameraLocationControl()
    {
        var stack = new MultiStackContainer()
        {
            Anchor = Alignment.Fill,
            ChildAnchor = Alignment.Fill,
            Orientation = Orientation.Vertical
        };


        stack.AddRow(
            _posX = new AutoUpdatingTextInput(() => _camera?.Position.X.ToString("F2"), v =>
            {
                if (_camera != null && float.TryParse(v, out var f))
                    _camera.Position = new Vector3(f, _camera.Position.Y, _camera.Position.Z);
            }),
            _posY = new AutoUpdatingTextInput(() => _camera?.Position.Y.ToString("F2"), v =>
            {
                if (_camera != null && float.TryParse(v, out var f))
                    _camera.Position = new Vector3(_camera.Position.X, f, _camera.Position.Z);
            }),
            _posZ = new AutoUpdatingTextInput(() => _camera?.Position.Z.ToString("F2"), v =>
            {
                if (_camera != null && float.TryParse(v, out var f))
                    _camera.Position = new Vector3(_camera.Position.X, _camera.Position.Y, f);
            })
        );
        stack.AddRow(
            _movementSpeed = new AutoUpdatingTextInput(
                () => _cameraController?.MoveSpeed.ToString("F2"),
                (v) =>
                {
                    if (_cameraController != null && float.TryParse(v, out var f))
                    {
                        _cameraController.MoveSpeed = f;
                    }
                })
        );
        stack.AddRow(
            _rotationSpeed = new AutoUpdatingTextInput(
                () => _cameraController?.RotateSpeed.ToString("F2"),
                (v) =>
                {
                    if (_cameraController != null && float.TryParse(v, out var f))
                    {
                        _cameraController.RotateSpeed = f;
                    }
                })
        );
        AddChild(stack);
    }

    protected override void OnInit(IGuiRenderer renderer)
    {
        base.OnInit(renderer);
        var services = ((IGame)GuiManager.Game).ServiceProvider;
        _camera = services.GetRequiredService<ICamera>();
        _cameraController = services.GetOrCreateInstance<CameraKeyboardController>();
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        base.OnUpdate(gameTime);
    }
}