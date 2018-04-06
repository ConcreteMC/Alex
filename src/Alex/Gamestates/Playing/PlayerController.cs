using System;
using Alex.Blocks;
using Alex.Entities;
using Alex.Rendering.Camera;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;

namespace Alex.Gamestates.Playing
{
    public class PlayerController
    {
        public const float Gravity = 0.08f;
        public const float DefaultDrag = 0.02f;
        public const float Acceleration = 0.02f;

        public const float MouseSpeed = 0.25f;

	    private float FlyingSpeed = 10f;

        private MouseState PreviousMouseState { get; set; }
        private float _leftrightRot = MathHelper.PiOver2;
        private float _updownRot = -MathHelper.Pi / 10.0f;

        public bool IsJumping { get; private set; }
        public bool IsFreeCam { get; set; }
        private Vector3 Velocity { get; set; }
       // private Vector3 Drag { get; set; }

        private GraphicsDevice Graphics { get; }
        private World World { get; }
        private Settings GameSettings { get; }

		private Player Player { get; }
		public PlayerController(GraphicsDevice graphics, World world, Settings settings, Player player)
		{
			Player = player;
            Graphics = graphics;
            World = world;
            GameSettings = settings;

            IsFreeCam = true;

            Velocity = Vector3.Zero;

            PreviousMouseState = Mouse.GetState();
		}

		private bool _inActive = true;
	    public bool CheckInput { get; set; } = false;
	    private KeyboardState _prevKeyState;
        public void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

	        bool isSprinting = false;
            bool originalJumpValue = IsJumping;
            var moveVector = Vector3.Zero;
            if (CheckInput)
            {
                var currentKeyboardState = Keyboard.GetState();

	            if (currentKeyboardState != _prevKeyState)
	            {
		            if (currentKeyboardState.IsKeyDown(KeyBinds.ToggleFreeCam))
		            {
			            IsFreeCam = !IsFreeCam;
		            }

		            _prevKeyState = currentKeyboardState;
	            }

	            Player.IsSprinting = currentKeyboardState.IsKeyDown(KeyBinds.Down);

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

	                if (currentKeyboardState.IsKeyDown(KeyBinds.IncreaseSpeed))
		                FlyingSpeed += 1;

	                if (currentKeyboardState.IsKeyDown(KeyBinds.DecreaseSpeed))
		                FlyingSpeed -= 1;

	                if (currentKeyboardState.IsKeyDown(KeyBinds.ResetSpeed))
		                FlyingSpeed = 10f;
                }
				else
                {
                    if (currentKeyboardState.IsKeyDown(KeyBinds.Up) && !IsJumping)
                    {
	                    moveVector.Y = 1;
                    }
                }
            }

	        if (CheckInput)
	        {
		        if (_inActive)
		        {
			        _inActive = false;
					Mouse.SetPosition(Graphics.Viewport.Width / 2, Graphics.Viewport.Height / 2);
			        PreviousMouseState = Mouse.GetState();
		        }
		        MouseState currentMouseState = Mouse.GetState();
		        if (currentMouseState != PreviousMouseState)
		        {
			        float xDifference = currentMouseState.X - PreviousMouseState.X;
			        float yDifference = currentMouseState.Y - PreviousMouseState.Y;

			        float mouseModifier = (float) (MouseSpeed * GameSettings.MouseSensitivy);

			        _leftrightRot -= mouseModifier * xDifference * dt;
			        _updownRot -= mouseModifier * yDifference * dt;
			        _updownRot = MathHelper.Clamp(_updownRot, MathHelper.ToRadians(-89.0f),
				        MathHelper.ToRadians(89.0f));

			        World.Player.KnownPosition.Pitch = MathHelper.ToDegrees(-_updownRot);
			        World.Player.KnownPosition.HeadYaw = MathHelper.ToDegrees(MathHelper.WrapAngle(_leftrightRot));
			        //World.Camera.Rotation = new Vector3(-_updownRot, MathHelper.WrapAngle(_leftrightRot), 0);
		        }

		        Mouse.SetPosition(Graphics.Viewport.Width / 2, Graphics.Viewport.Height / 2);

		        PreviousMouseState = Mouse.GetState();
	        }
	        else if (!_inActive)
	        {
		        _inActive = true;
	        }

	        DoPhysics(originalJumpValue, Player.IsSprinting, moveVector, dt);
		}

        private void DoPhysics(bool originalJumpValue, bool sprinting, Vector3 direction, float dt)
        {
	        var oldVelocity = new Vector3(Velocity.X, Velocity.Y, Velocity.Z);

			float currentDrag = GetCurrentDrag();
			var drag = new Vector3(1f - currentDrag, 1f - currentDrag, 1f - currentDrag);

			float speedFactor = (float)Player.MovementSpeed;

			if (sprinting)
	        {
		        speedFactor += 0.2806f;
	        }

	        if (!IsFreeCam)
	        {
		        bool onGround = false;
		        if (!IsOnGround(Velocity))
		        {
					Velocity -= new Vector3(0, (float)Player.Gravity, 0);

			        if (Velocity.Y < -3.92f)
			        {
				        Velocity = new Vector3(Velocity.X, -3.92f, Velocity.Z);
			        }
				}
		        else
		        {
			        onGround = true;

			        if (direction.Y > 0 && !IsJumping)
			        {
				        direction.Y = 0;
				        Velocity += new Vector3(0, 0.42f, 0);
				        IsJumping = true;
			        }
			        else
			        {
						if (Velocity.Y < 0)
				        {
					        Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
					        IsJumping = false;
						}
			        }
		        }

		        Player.KnownPosition.OnGround = onGround;
			}
	        else
	        {
		        speedFactor = (float) Player.FlyingSpeed;
		        speedFactor *= 2.5f;

				if (direction.Y > 0)
		        {
			        direction.Y = 0;
					Player.KnownPosition.Move(new Vector3(0, speedFactor * dt, 0));
		        }
				else if (direction.Y < 0)
		        {
			        direction.Y = 0;
			        Player.KnownPosition.Move(new Vector3(0, -speedFactor * dt, 0));
				}
			}

	        var groundSpeedSquared = Velocity.X * Velocity.X + Velocity.Z * Velocity.Z;
	        if (groundSpeedSquared > (4.7f))
	        {
		        var correctionScale = (float)Math.Sqrt(4.7f / groundSpeedSquared);
		        Velocity *= new Vector3(correctionScale, 1f, correctionScale);
	        }

	        Velocity += (direction * speedFactor);

			if (Velocity.LengthSquared() < 0.001f)
	        {
				Velocity = Vector3.Zero;
	        }

	        Velocity *= drag;

			var v = ((oldVelocity + Velocity) * 0.5f) * dt;
			if (v != Vector3.Zero) //Only if we moved.
			{
                var preview = World.Player.KnownPosition.PreviewMove(v).Floor();

				var headBlockPos = preview;
				headBlockPos += new Vector3(0, 1f, 0);

                var headBlock = (Block)World.GetBlock(headBlockPos);
                var headBoundingBox = headBlock.GetBoundingBox(headBlockPos);

                var feetBlockPosition = preview;
                var feetBlock = (Block)World.GetBlock(feetBlockPosition);
                var feetBoundingBox = feetBlock.GetBoundingBox(feetBlockPosition);

				var difference = (feetBoundingBox.Max.Y) - (preview.Y);
				//Log.Debug($"{difference}");
                var playerBoundingBox = GetPlayerBoundingBox(preview);

                if (!headBlock.Solid && !IsColiding(playerBoundingBox, headBoundingBox) &&
                    !feetBlock.Solid && !IsColiding(playerBoundingBox, feetBoundingBox))
                {
	                World.Player.KnownPosition.Move(v);
                }
                else if (!headBlock.Solid && !IsColiding(playerBoundingBox, headBoundingBox) && feetBlock.Solid &&
                         (difference <= 0.5f))
                {
					World.Player.KnownPosition.Move((v) + new Vector3(0, Math.Abs(difference), 0));
                }
                else
                {
					Velocity = Vector3.Zero;
                }
            }
        }

        private float GetCurrentDrag()
        {
	        return (float)Player.Drag;
            Vector3 applied = World.Camera.Position.Floor();
            applied -= new Vector3(0, Player.EyeLevel, 0);

            if (applied.Y > 255) return DefaultDrag;
            if (applied.Y < 0) return DefaultDrag;

            return World.GetBlock(applied.X, applied.Y, applied.Z).Drag;
        }

        private bool IsOnGround(Vector3 velocity)
        {
	        var playerPosition = World.Player.KnownPosition;


			Vector3 applied = playerPosition.ToVector3().Floor();
            //applied -= new Vector3(0, Player.EyeLevel - 1, 0);

            if (applied.Y > 255) return false;
            if (applied.Y < 0) return false;

            var block = (Block)World.GetBlock(applied.X, applied.Y, applied.Z);
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
