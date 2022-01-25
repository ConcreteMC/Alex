using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Attributes;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace ResourcePackLib.ModelExplorer.Components;

public class CameraKeyboardController : GameComponent
{
    private const Keys RotateModifier = Keys.LeftAlt;
    private const Keys FastModifier = Keys.LeftControl;
    private const Keys SuperFastModifier = Keys.LeftShift;

    public float RotateSpeed { get; set; } = 45f;
    public float MoveSpeed { get; set; } = 2f;
    
    [Service] protected InputManager InputManager { get; private set; }
    [Service] protected ICamera Camera { get; private set; }

    private KeyboardInputListener _keyboardListener;
    public CameraKeyboardController(IGame game) : base(game.Game)
    {
        
    }
    public override void Initialize()
    {
        base.Initialize();
        InputManager.GetOrAddPlayerManager(PlayerIndex.One).TryGetListener<KeyboardInputListener>(out _keyboardListener);
    }
    
    public override void Update(GameTime gameTime)
    {
        var rotateSpeed = RotateSpeed;
        var moveSpeed = MoveSpeed;
        
        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(FastModifier))
        {
            moveSpeed *= 5;
            rotateSpeed *= 5;
        }        
        if (keyboard.IsKeyDown(SuperFastModifier))
        {
            moveSpeed *= 10;
            rotateSpeed *= 10;
        }
        
        if (keyboard.IsKeyDown(RotateModifier))
        {
            var rotateDiff = (float)((gameTime.ElapsedGameTime.TotalSeconds / 1f) * rotateSpeed);
            if (_keyboardListener.IsAnyDown(Keys.NumPad8)) Camera.Rotation += (Vector3.UnitX * MathHelper.ToRadians(-rotateDiff));
            if (_keyboardListener.IsAnyDown(Keys.NumPad2)) Camera.Rotation += (Vector3.UnitX * MathHelper.ToRadians(rotateDiff));
            if (_keyboardListener.IsAnyDown(Keys.NumPad4)) Camera.Rotation += (Vector3.UnitY * MathHelper.ToRadians(-rotateDiff));
            if (_keyboardListener.IsAnyDown(Keys.NumPad6)) Camera.Rotation += (Vector3.UnitY * MathHelper.ToRadians(rotateDiff));
        }
        else if (keyboard.IsKeyDown(Keys.Home))
        {
            Camera.Position = Vector3.Zero;
            Camera.Rotation = Vector3.Zero;
        }
        else
        {
            var moveDiff = Vector3.One * (float)((gameTime.ElapsedGameTime.TotalSeconds / 1f) * moveSpeed);
            if (keyboard.IsKeyDown(Keys.W)) Camera.MoveRelative(Vector3.Forward * moveDiff);
            if (keyboard.IsKeyDown(Keys.A)) Camera.MoveRelative(Vector3.Left * moveDiff);
            if (keyboard.IsKeyDown(Keys.S)) Camera.MoveRelative(Vector3.Backward * moveDiff);
            if (keyboard.IsKeyDown(Keys.D)) Camera.MoveRelative(Vector3.Right * moveDiff);
            if (keyboard.IsKeyDown(Keys.Q)) Camera.MoveRelative(Vector3.Up * moveDiff);
            if (keyboard.IsKeyDown(Keys.E)) Camera.MoveRelative(Vector3.Down * moveDiff);
        }
    }
}