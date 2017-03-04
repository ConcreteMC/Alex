using Alex.Entities;
using Alex.Rendering;
using Alex.Rendering.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex.Gamestates.Playing
{
    public class CameraComponent
    {
        public const float Gravity = 0.02f;
        public const float DefaultDrag = 0.8f;
        public const float Acceleration = 0.02f;

        public const float MouseSpeed = 0.25f;

        private MouseState PreviousMouseState { get; set; }
        private float _leftrightRot = MathHelper.PiOver2;
        private float _updownRot = -MathHelper.Pi / 10.0f;

        public bool IsJumping { get; private set; }
        public bool IsFreeCam { get; set; }
        private Vector3 Velocity { get; set; }
        private Vector3 Drag { get; set; }

        private FirstPersonCamera Camera { get; }
        private GraphicsDevice Graphics { get; }
        private World World { get; }
        private Settings GameSettings { get; }
        public CameraComponent(FirstPersonCamera camera, GraphicsDevice graphics, World world, Settings settings)
        {
            Camera = camera;
            Graphics = graphics;
            World = world;
            GameSettings = settings;

            IsFreeCam = true;

            Velocity = Vector3.Zero;
            Drag = Vector3.Zero;

            Mouse.SetPosition(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);
            PreviousMouseState = Mouse.GetState();
        }

        public void Update(GameTime gameTime, bool checkInput)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            bool originalJumpValue = IsJumping;
            var moveVector = Vector3.Zero;
            if (checkInput)
            {
                var currentKeyboardState = Keyboard.GetState();
                if (currentKeyboardState.IsKeyDown(KeyBinds.Forward))
                    moveVector.Z = 1;

                if (currentKeyboardState.IsKeyDown(KeyBinds.Backward))
                    moveVector.Z = -1;

                if (currentKeyboardState.IsKeyDown(KeyBinds.Left))
                    moveVector.X = 1;

                if (currentKeyboardState.IsKeyDown(KeyBinds.Right))
                    moveVector.X = -1;

                if (IsFreeCam)
                {
                    if (currentKeyboardState.IsKeyDown(KeyBinds.Up))
                        moveVector.Y = 1;

                    if (currentKeyboardState.IsKeyDown(KeyBinds.Down))
                        moveVector.Y = -1;
                }
                else
                {
                    if (currentKeyboardState.IsKeyDown(KeyBinds.Up) && !IsJumping && IsOnGround(Velocity))
                    {
                        Velocity += new Vector3(0, Gravity, 0);
                        IsJumping = true;
                    }
                }
            }

            if (!IsFreeCam)
            {
               DoPhysics(originalJumpValue, moveVector, dt);
            }
            else
            {
                if (moveVector != Vector3.Zero) // If we moved
                {
                    moveVector *= 10f * dt;

                    Camera.Move(moveVector);
                }
            }

            if (checkInput)
            {
                MouseState currentMouseState = Mouse.GetState();
                if (currentMouseState != PreviousMouseState)
                {
                    float xDifference = currentMouseState.X - PreviousMouseState.X;
                    float yDifference = currentMouseState.Y - PreviousMouseState.Y;

                    float mouseModifier = (float)(MouseSpeed * GameSettings.MouseSensitivy);

                    _leftrightRot -= mouseModifier * xDifference * dt;
                    _updownRot -= mouseModifier * yDifference * dt;

                   Camera.Rotation = new Vector3(-MathHelper.Clamp(_updownRot, MathHelper.ToRadians(-90.0f),
                        MathHelper.ToRadians(75.0f)), MathHelper.WrapAngle(_leftrightRot), 0);
                }
                // Cursor.Position = new Point(Window.Position.X + GraphicsDevice.Viewport.Width/2,
                //    Window.Position.Y + GraphicsDevice.Viewport.Height/2);

                Mouse.SetPosition(Graphics.Viewport.Width / 2, Graphics.Viewport.Height / 2);

                PreviousMouseState = Mouse.GetState();
            }
        }

        private void DoPhysics(bool originalJumpValue, Vector3 moveVector, float dt)
        {
            //Apply Gravity.
            Velocity += new Vector3(0, -Gravity * dt, 0);

            float currentDrag = GetCurrentDrag();
            bool onGround = false;
            if (IsOnGround(Velocity))
            {
                onGround = true;
                if (originalJumpValue == IsJumping)
                {
                    Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                    IsJumping = false;
                }
            }

            Drag = -Velocity * currentDrag;
            Velocity += (Drag + (moveVector * Acceleration)) * dt;

            //if (Velocity != Vector3.Zero)
            //	Logging.Info("Velocity: " + Velocity + "(DT is " + dt + ")");

            if (Velocity != Vector3.Zero) //Only if we moved.
            {
                var preview = Camera.PreviewMove(Velocity);

                var headBlock = World.GetBlock(preview);
                var headBoundingBox = headBlock.GetBoundingBox(preview.Floor());

                var feetBlockPosition = preview.Floor() - new Vector3(0, 1, 0);
                var feetBlock = World.GetBlock(feetBlockPosition);
                var feetBoundingBox = feetBlock.GetBoundingBox(feetBlockPosition);

                var difference = (preview.Y) - (feetBlockPosition.Y + feetBlock.BlockModel.Size.Y);

                var playerBoundingBox = GetPlayerBoundingBox(preview);

                if (!headBlock.Solid && !IsColiding(playerBoundingBox, headBoundingBox) &&
                    !feetBlock.Solid && !IsColiding(playerBoundingBox, feetBoundingBox))
                {
                    Camera.Move(Velocity);
                }
                else if (!headBlock.Solid && !IsColiding(playerBoundingBox, headBoundingBox) && feetBlock.Solid &&
                         (difference <= 0.5f))
                {
                    Camera.Move(Velocity + new Vector3(0, feetBlock.BlockModel.Size.Y, 0));
                }
            }
        }

        private float GetCurrentDrag()
        {
            Vector3 applied = Camera.Position.Floor();
            applied -= new Vector3(0, Player.EyeLevel, 0);

            if (applied.Y > 255) return DefaultDrag;
            if (applied.Y < 0) return DefaultDrag;

            return World.GetBlock(applied.X, applied.Y, applied.Z).Drag;
        }

        private bool IsOnGround(Vector3 velocity)
        {
            var playerPosition = Camera.Position;

            Vector3 applied = Camera.Position.Floor();
            applied -= new Vector3(0, Player.EyeLevel, 0);

            if (applied.Y > 255) return false;
            if (applied.Y < 0) return false;

            var block = World.GetBlock(applied.X, applied.Y, applied.Z);
            var boundingBox = block.GetBoundingBox(applied);

            if (block.Solid)
            {
                if (IsColidingGravity(GetPlayerBoundingBox(playerPosition), boundingBox))
                {
                    return true;
                }

                if (IsColidingGravity(GetPlayerBoundingBox(playerPosition + velocity), boundingBox))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsColidingGravity(BoundingBox box, BoundingBox blockBox)
        {
            return box.Min.Y >= blockBox.Min.Y;
        }

        private bool IsColiding(BoundingBox box, BoundingBox blockBox)
        {
            var a = new System.Drawing.Rectangle((int)box.Min.X, (int)box.Min.Z, (int)(box.Max.X - box.Min.X), (int)(box.Max.Z - box.Min.Z));
            var b = new System.Drawing.Rectangle((int)blockBox.Min.X, (int)blockBox.Min.Z, (int)(blockBox.Max.X - blockBox.Min.X), (int)(blockBox.Max.Z - blockBox.Min.Z));
            return a.IntersectsWith(b);
        }

        private BoundingBox GetPlayerBoundingBox(Vector3 position)
        {
            return new BoundingBox(position - new Vector3(0.15f, 0, 0.15f), position + new Vector3(0.15f, 1.8f, 0.15f));
        }
    }
}
