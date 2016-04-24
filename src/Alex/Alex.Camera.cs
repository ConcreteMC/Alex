using System;
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
        private readonly float Gravity = -9.8f;
        private int _distanceIndex;

        /// <summary>
        ///     Are we jumping right now?
        /// </summary>
        public bool IsJumping { get; private set; }

        public bool IsFreeCam { get; set; }

        private Vector3 Velocity { get; set; }

        internal void InitCamera()
        {
            IsFreeCam = true;

            Velocity = new Vector3();
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
            //var cam = (FirstPersonCamera) Game.GetCamera();

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
						Velocity += new Vector3(0, Math.Abs(Gravity) * 2, 0);
						IsJumping = true;
                    }
                }
            }

		    if (!IsFreeCam)
		    {
                Velocity += new Vector3(0, Gravity, 0);
		        Velocity *= dt;

		        var playerPosition = Game.GetCamera().Position;

                Vector3 applied = Game.GetCamera().Position;
		        applied -= new Vector3(0, Player.EyeLevel, 0);

                var block = World.GetBlock(applied.X, applied.Y, applied.Z);
		        var boundingBox = block.GetBoundingBox(applied);

		        if (block.Solid)
		        {
		            if (IsColidingGravity(GetPlayerBoundingBox(playerPosition + Velocity), boundingBox))
		            {
		                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
		                if (IsJumping) IsJumping = false;
		            }
		        }

		        if (moveVector != Vector3.Zero) // If we moved
                {
                    moveVector *= MovementSpeed * dt;

                    var preview = ((FirstPersonCamera) Game.GetCamera()).PreviewMove(moveVector);
                    block = World.GetBlock(preview);
                    boundingBox = block.GetBoundingBox(preview);

                    var b2Pos = preview - new Vector3(0, 1, 0);
                    var block2 = World.GetBlock(b2Pos);
                    var boundingBox2 = block.GetBoundingBox(b2Pos);

                    var difference = (preview.Y) - (b2Pos.Y + block2.BlockModel.Size.Y);

                    // block2.BlockModel.Size.Y
                    if (!block.Solid && !IsColiding(GetPlayerBoundingBox(preview), boundingBox) &&
                        !block2.Solid && !IsColiding(GetPlayerBoundingBox(preview), boundingBox2))
                    {
                        ((FirstPersonCamera) Game.GetCamera()).Move(moveVector);
                    }
                    else if (!block.Solid && !IsColiding(GetPlayerBoundingBox(preview), boundingBox) && block2.Solid &&
                             (difference >= 0.5f))
                    {
                        ((FirstPersonCamera) Game.GetCamera()).Move(moveVector +
                                                                    new Vector3(0, block2.BlockModel.Size.Y, 0));
                    }
                }

                if (Velocity != Vector3.Zero)
                {
                    Game.GetCamera().Position += Velocity;
                }
            }
		    else
		    {
		        if (moveVector != Vector3.Zero) // If we moved
		        {
		            moveVector *= MovementSpeed*dt;

		            ((FirstPersonCamera) Game.GetCamera()).Move(moveVector);
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

            /*            if (CurrentKeyboardState.IsKeyUp(KeyBinds.Fog) && PrevKeyboardState.IsKeyDown(KeyBinds.Fog))
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
            }*/
        }

        private bool IsColidingGravity(BoundingBox box, BoundingBox blockBox)
        {
            return box.Min.Y >= blockBox.Min.Y;
        }

        private bool IsColiding(BoundingBox box, BoundingBox blockBox)
        {
            var a = new System.Drawing.Rectangle((int) box.Min.X, (int) box.Min.Z, (int) (box.Max.X - box.Min.X), (int) (box.Max.Z - box.Min.Z));
            var b = new System.Drawing.Rectangle((int)blockBox.Min.X, (int)blockBox.Min.Z, (int)(blockBox.Max.X - blockBox.Min.X), (int)(blockBox.Max.Z - blockBox.Min.Z));
            return a.IntersectsWith(b);
            /* if (box.Min.X <= blockBox.Max.X && box.Min.X >= blockBox.Min.X)
             {
                 if (box.Min.Z <= blockBox.Max.Z && box.Min.Z >= blockBox.Min.Z)
                 {
                     return true;
                 }
             }
             return false; */
        }

	    private BoundingBox GetPlayerBoundingBox(Vector3 position)
	    {
		    return new BoundingBox(position, position + new Vector3(0.3f, 1.8f, 0.3f));
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

        private class GravityVector
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }

            public GravityVector()
            {
                X = 0;
                Y = 0;
                Z = 0;
            }
        }
    }
}
 