using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Attributes;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace ResourcePackLib.ModelExplorer.Components;

public class CameraMouseController : GameComponent
{
    
    [Service] protected InputManager InputManager { get; private set; }
    [Service] protected ICamera Camera { get; private set; }

    public bool InvertX { get; set; }
    public bool InvertY { get; set; }
    public double Sensitivity { get; set; } = 180d;

    private Vector3 _rotation = Vector3.Zero;
    
    private Vector3 _previous;
    private MouseInputListener _cursorInputListener;
    
    public CameraMouseController(IGame game) : base(game.Game)
    {
        
    }

    public override void Initialize()
    {
        base.Initialize();
        InputManager.GetOrAddPlayerManager(PlayerIndex.One).TryGetListener<MouseInputListener>(out _cursorInputListener);
    }

    public override void Update(GameTime gameTime)
    {
        if (!_cursorInputListener.IsAnyDown(MouseButton.Right))
        {
            return;
        }
        var cursor = _cursorInputListener.GetCursorRay();
        var p = cursor.Position;

        if (_cursorInputListener.IsAnyBeginPress(MouseButton.Right))
        {
            _previous = p;
        }
        
        var c = new Vector3(Camera.Viewport.Bounds.Center.ToVector2(), 0f);

        var delta = p - _previous;
        if(delta.LengthSquared() < 10f) return;
        
        delta *= (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        var lookDelta = delta * (float)Sensitivity;

        _rotation -= new Vector3(MathHelper.ToRadians(lookDelta.X), MathHelper.ToRadians(lookDelta.Y), MathHelper.ToRadians(lookDelta.Z));
        //_rotation.Normalize();
        
        Camera.Rotation = Quaternion.CreateFromYawPitchRoll(_rotation.X, _rotation.Y, _rotation.Z);
        
        _previous = p;
    }
}