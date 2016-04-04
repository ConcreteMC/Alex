using System.Drawing;
using System.Windows.Forms;
using Alex.Entities;
using Alex.Rendering.Camera;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Game = Alex.Game;
using Point = System.Drawing.Point;

namespace Alex
{
    public partial class Alex
    {
        private readonly float Gravity = -13f;
        private int _distanceIndex;

        /// <summary>
        ///     Are we jumping right now?
        /// </summary>
        public bool IsJumping { get; private set; }

        public bool IsFreeCam { get; set; }

        internal void InitCamera()
        {
            IsFreeCam = true;

            PrevKeyboardState = Keyboard.GetState();

			Cursor.Position = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
			_originalMouseState = Mouse.GetState();
        }


		private MouseState _originalMouseState;
		private float _leftrightRot = MathHelper.PiOver2;
		private float _updownRot = -MathHelper.Pi / 10.0f;

		internal void UpdateCamera(GameTime gameTime)
        {
			var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var cam = (FirstPersonCamera) Game.GetCamera();

            CurrentKeyboardState = Keyboard.GetState();

            var moveVector = Vector3.Zero;
            if (IsActive)
            {
                if (CurrentKeyboardState.IsKeyDown(KeyBinds.Forward))
                    moveVector.Z = 1;

                if (CurrentKeyboardState.IsKeyDown(KeyBinds.Backward))
                    moveVector.Z = -1;

                if (CurrentKeyboardState.IsKeyDown(KeyBinds.Left))
                    moveVector.X = 1;

                if (CurrentKeyboardState.IsKeyDown(KeyBinds.Right))
                    moveVector.X = -1;

                if (IsFreeCam)
                {
                    if (CurrentKeyboardState.IsKeyDown(KeyBinds.Up))
                        moveVector.Y = 1;

                    if (CurrentKeyboardState.IsKeyDown(KeyBinds.Down))
                        moveVector.Y = -1;
                }
                else
                {
                    if (CurrentKeyboardState.IsKeyDown(KeyBinds.Up) && !IsJumping)
                    {
                        moveVector.Y = 12;
                        IsJumping = true;
                    }
                }
            }

            if (CurrentKeyboardState.IsKeyUp(KeyBinds.Fog) && PrevKeyboardState.IsKeyDown(KeyBinds.Fog))
            {
                _distanceIndex++;
                if (_distanceIndex >= cam.Distances.Length)
                    _distanceIndex = 0;

                ((FirstPersonCamera)Game.GetCamera()).FarDistance = cam.Distances[_distanceIndex];
                ((FirstPersonCamera)Game.GetCamera()).ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4,
                    GraphicsDevice.Viewport.AspectRatio,
                    FirstPersonCamera.NearDistance,
                    ((FirstPersonCamera)Game.GetCamera()).FarDistance);
            }

            if (moveVector != Vector3.Zero) // If we moved
            {
                moveVector *= MovementSpeed * dt;
                ((FirstPersonCamera)Game.GetCamera()).Move(moveVector);
            }

            if (!IsFreeCam)
            {
                // Now try applying gravity
                var gravityVector = Vector3.Zero;
                gravityVector.Y += Gravity;

                gravityVector *= dt;

                // Add the player's eye level.
                var vectorWithFeet = new Vector3(gravityVector.X, gravityVector.Y - Player.EyeLevel, gravityVector.Z);
                var gravLoc = ((FirstPersonCamera) Game.GetCamera()).PreviewMove(vectorWithFeet);
                var worldLoc = gravLoc.ToBlockCoords();

                if (World.GetBlock(worldLoc.X, worldLoc.Y, worldLoc.Z).Solid)
                {
                    if (IsJumping)
                        IsJumping = false;
                }
                else
                {
                    ((FirstPersonCamera) Game.GetCamera()).Move(gravityVector);
                }
            }

            if (IsActive)
            {
				MouseState currentMouseState = Mouse.GetState();
				if (currentMouseState != _originalMouseState)
				{
					float xDifference = currentMouseState.X - _originalMouseState.X;
					float yDifference = currentMouseState.Y - _originalMouseState.Y;

					_leftrightRot -= MouseSpeed * xDifference * dt;
					_updownRot -= MouseSpeed * yDifference * dt;

					Game.GetCamera().Rotation = new Vector3(-MathHelper.Clamp(_updownRot, MathHelper.ToRadians(-75.0f),
							MathHelper.ToRadians(90.0f)), MathHelper.WrapAngle(_leftrightRot), 0);
				}

				Cursor.Position = new Point(Window.Position.X + GraphicsDevice.Viewport.Width / 2, Window.Position.Y + GraphicsDevice.Viewport.Height / 2);
	            _originalMouseState = Mouse.GetState();
	            // _originalMouseState = currentMouseState;
            }

            PrevKeyboardState = CurrentKeyboardState;
        }

        #region Keyboard

        private KeyboardState PrevKeyboardState { get; set; }
        private KeyboardState CurrentKeyboardState { get; set; }

        #endregion

        #region Constants

        /// <summary>
        ///     The mouse movement speed
        /// </summary>
        public const float MouseSpeed = 0.1f;

        /// <summary>
        ///     The camera's movement speed
        /// </summary>
        public const float MovementSpeed = 10f;

        #endregion
    }
}