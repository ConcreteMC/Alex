using System;
using System.Windows.Forms;
using Alex.Entities;
using Alex.Rendering.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Point = System.Drawing.Point;

namespace Alex
{
    public partial class Alex
    {
	    public const float Gravity = 0.02f;
	    public const float DefaultDrag = 0.8f;
	    public const float Acceleration = 0.02f;

		/// <summary>
		///     Are we jumping right now?
		/// </summary>
		public bool IsJumping { get; private set; }

        public bool IsFreeCam { get; set; }

        private Vector3 Velocity { get; set; }
		private Vector3 Drag { get; set; }

        internal void InitCamera()
        {
			Mouse.WindowHandle = Window.Handle;
			IsFreeCam = true;

            Velocity = new Vector3();
			Drag = new Vector3();
            PrevKeyboardState = Keyboard.GetState();

			Mouse.SetPosition(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
			_originalMouseState = Mouse.GetState();
        }


		private MouseState _originalMouseState;
		private float _leftrightRot = MathHelper.PiOver2;
		private float _updownRot = -MathHelper.Pi / 10.0f;

	    internal void UpdateCamera(GameTime gameTime, bool checkInput)
	    {
		    var dt = (float) gameTime.ElapsedGameTime.TotalSeconds;

		    bool originalJumpValue = IsJumping;
		    var moveVector = Vector3.Zero;
		    if (checkInput)
		    {
				CurrentKeyboardState = Keyboard.GetState();
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
				    if (CurrentKeyboardState.IsKeyDown(KeyBinds.Up) && !IsJumping && IsOnGround(Velocity))
				    {
					    Velocity += new Vector3(0, Gravity, 0);
					    IsJumping = true;
				    }
			    }
		    }

		    if (!IsFreeCam)
		    {
			    //Apply Gravity.
			    Velocity += new Vector3(0, -Gravity*dt, 0);

			    float currentDrag = GetCurrentDrag();
				if (IsOnGround(Velocity))
			    {
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
				    var preview = ((FirstPersonCamera) Game.GetCamera()).PreviewMove(Velocity);
				    var block = World.GetBlock(preview);
				    var boundingBox = block.GetBoundingBox(preview.Floor());

				    var b2Pos = preview.Floor() - new Vector3(0, 1, 0);
				    var block2 = World.GetBlock(b2Pos);
				    var boundingBox2 = block.GetBoundingBox(b2Pos);

				    var difference = (preview.Y) - (b2Pos.Y + block2.BlockModel.Size.Y);

				    if (!block.Solid && !IsColiding(GetPlayerBoundingBox(preview), boundingBox) &&
				        !block2.Solid && !IsColiding(GetPlayerBoundingBox(preview), boundingBox2))
				    {
					    ((FirstPersonCamera) Game.GetCamera()).Move(Velocity);
				    }
				    else if (!block.Solid && !IsColiding(GetPlayerBoundingBox(preview), boundingBox) && block2.Solid &&
				             (difference <= 0.5f))
				    {
					    ((FirstPersonCamera) Game.GetCamera()).Move(Velocity +
					                                                new Vector3(0, block2.BlockModel.Size.Y, 0));
				    }
			    }
		    }
		    else
		    {
			    if (moveVector != Vector3.Zero) // If we moved
			    {
				    moveVector *= 10f*dt;

				    ((FirstPersonCamera) Game.GetCamera()).Move(moveVector);
			    }
		    }

		    if (checkInput)
		    {
				MouseState currentMouseState = Mouse.GetState();
			    if (currentMouseState != _originalMouseState)
			    {
				    float xDifference = currentMouseState.X - _originalMouseState.X;
				    float yDifference = currentMouseState.Y - _originalMouseState.Y;

			        float mouseModifier = (float) (MouseSpeed*GameSettings.MouseSensitivy);

				    _leftrightRot -= mouseModifier*xDifference*dt;
				    _updownRot -= mouseModifier*yDifference*dt;

				    Game.GetCamera().Rotation = new Vector3(-MathHelper.Clamp(_updownRot, MathHelper.ToRadians(-90.0f),
					    MathHelper.ToRadians(75.0f)), MathHelper.WrapAngle(_leftrightRot), 0);
			    }
			    Cursor.Position = new Point(Window.Position.X + GraphicsDevice.Viewport.Width/2,
				    Window.Position.Y + GraphicsDevice.Viewport.Height/2);

				//Mouse.SetPosition(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);

				_originalMouseState = Mouse.GetState();
		    }

		    PrevKeyboardState = CurrentKeyboardState;
	    }

	    private float GetCurrentDrag()
	    {
			Vector3 applied = Game.GetCamera().Position.Floor();
			applied -= new Vector3(0, Player.EyeLevel, 0);

			if (applied.Y > 255) return DefaultDrag;
			if (applied.Y < 0) return DefaultDrag;

		    return World.GetBlock(applied.X, applied.Y, applied.Z).Drag;
	    }

	    private bool IsOnGround(Vector3 velocity)
	    {
			var playerPosition = Game.GetCamera().Position;

			Vector3 applied = Game.GetCamera().Position.Floor();
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
            var a = new System.Drawing.Rectangle((int) box.Min.X, (int) box.Min.Z, (int) (box.Max.X - box.Min.X), (int) (box.Max.Z - box.Min.Z));
            var b = new System.Drawing.Rectangle((int)blockBox.Min.X, (int)blockBox.Min.Z, (int)(blockBox.Max.X - blockBox.Min.X), (int)(blockBox.Max.Z - blockBox.Min.Z));
            return a.IntersectsWith(b);
        }

	    private BoundingBox GetPlayerBoundingBox(Vector3 position)
	    {
		    return new BoundingBox(position - new Vector3(0.15f, 0, 0.15f), position + new Vector3(0.15f, 1.8f, 0.15f));
	    }

	    #region Keyboard

	private KeyboardState PrevKeyboardState { get; set; }
        private KeyboardState CurrentKeyboardState { get; set; }

        #endregion

        #region Constants

        /// <summary>
        ///     The mouse movement speed
        /// </summary>
        public const float MouseSpeed = 0.25f;

        #endregion
    }
}
 