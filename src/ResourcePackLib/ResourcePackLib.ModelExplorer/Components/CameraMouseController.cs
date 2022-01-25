using Microsoft.Xna.Framework;
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
    public double Sensitivity { get; set; } = 60d;
    
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

        //Camera.LookAt(Vector3.Zero);
        Camera.Rotation -= new Vector3(lookDelta.X, lookDelta.Y, 0f);
        //_rotation.Normalize();
        
        _previous = p;
    }
}