using System;
using Alex.API.Input;
using Alex.API.Input.Listeners;
using Alex.Entities;
using Alex.Gui.Dialogs.Containers;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;

namespace Alex.GameStates.Playing
{
    public class PlayerController
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PlayerController));

		public PlayerIndex PlayerIndex { get; }
		public PlayerInputManager InputManager { get; }
		public MouseInputListener MouseInputListener { get; }

        public bool IsFreeCam { get; set; }

        private GraphicsDevice Graphics { get; }
        private World World { get; }

        private Player Player { get; }
		private InputManager GlobalInputManager { get; }

		public PlayerController(GraphicsDevice graphics, World world, InputManager inputManager, Player player, PlayerIndex playerIndex)
		{
			Player = player;
            Graphics = graphics;
            World = world;
            PlayerIndex = playerIndex;

            IsFreeCam = true;

			GlobalInputManager = inputManager;
			InputManager = inputManager.GetOrAddPlayerManager(playerIndex);
			InputManager.AddListener(MouseInputListener = new MouseInputListener(playerIndex));
		}

		private bool _inActive = true;
	    public bool CheckInput { get; set; } = false;
	    private bool _allowMovementInput = true;
	    private bool IgnoreNextUpdate { get; set; } = false;
		private DateTime _lastForward = DateTime.UtcNow;
		private Vector2 _previousMousePosition = Vector2.Zero;

		private GuiPlayerInventoryDialog _guiPlayerInventoryDialog = null;

		public void Update(GameTime gameTime)
	    {
		   UpdatePlayerInput(gameTime);
	    }

	    private void UpdatePlayerInput(GameTime gt)
	    {
		    if (CheckInput)
		    {
				CheckGeneralInput(gt);
				CheckMovementInput(gt);
		    }
		    else if (!_inActive)
		    {
			    _inActive = true;
		    }
		}

	    private void CheckGeneralInput(GameTime gt)
	    {
		    if (InputManager.IsPressed(InputCommand.ToggleMenu))
		    {
			    
			}
			else if (InputManager.IsPressed(InputCommand.ToggleInventory))
			{
				if (_guiPlayerInventoryDialog == null)
				{
					Alex.Instance.GuiManager.ShowDialog(_guiPlayerInventoryDialog = new GuiPlayerInventoryDialog(Player, Player.Inventory));
				}
				else
				{
					Alex.Instance.GuiManager.HideDialog(_guiPlayerInventoryDialog);
					_guiPlayerInventoryDialog = null;
				}
			}
		}

	    public float LastSpeedFactor = 0f;
	    private void CheckMovementInput(GameTime gt)
	    {
		    if (!_allowMovementInput) return;

			var moveVector = Vector3.Zero;
			var now = DateTime.UtcNow;

		    if (InputManager.IsPressed(InputCommand.ToggleCameraFree))
		    {
			    IsFreeCam = !IsFreeCam;
		    }

		    float modifier = 1f;

			if (Player.IsInWater)
			{
				modifier = 0.3f;
			}
			else if (Player.IsSprinting && !Player.IsSneaking)
			{	
				modifier = 1.29997683577f;
			    //speedFactor *= 0.2806f; 
		    }
		    else if (Player.IsSneaking && !Player.IsSprinting)
		    {
			    modifier = 0.29997683576f;
		    }

		//	float speedFactor = (((float) Player.MovementSpeed) * modifier);
		    float speedFactor = (Player.IsFlying ? (float)Player.FlyingSpeed : (float) Player.MovementSpeed) * modifier;

			if (InputManager.IsDown(InputCommand.MoveForwards))
			{
				moveVector.Z += 1;
				if (!Player.IsSprinting)
				{
					if (InputManager.IsBeginPress(InputCommand.MoveForwards) &&
						now.Subtract(_lastForward).TotalMilliseconds <= 100)
					{
						Player.IsSprinting = true;
					}
				}

				_lastForward = now;
			}
			else
			{
				if (Player.IsSprinting)
				{
					Player.IsSprinting = false;
				}
			}

			if (InputManager.IsDown(InputCommand.MoveBackwards))
				moveVector.Z -= 1;

			if (InputManager.IsDown(InputCommand.MoveLeft))
				moveVector.X += 1;

			if (InputManager.IsDown(InputCommand.MoveRight))
				moveVector.X -= 1;

			if (Player.IsFlying)
			{
				//speedFactor *= 1f + (float)Player.FlyingSpeed;
				//speedFactor *= 2.5f;

				if (InputManager.IsDown(InputCommand.MoveUp))
					moveVector.Y += 1;

				if (InputManager.IsDown(InputCommand.MoveDown))
				{
					moveVector.Y -= 1;
					Player.IsSneaking = true;
				}
				else
				{
					Player.IsSneaking = false;
				}
			}
			else
			{
				if (InputManager.IsDown(InputCommand.MoveUp))
				{
					if (Player.IsInWater)
					{
						moveVector.Y += 0.04f;
					}
					else
					{
						if (Player.KnownPosition.OnGround && Math.Abs(Math.Floor(Player.KnownPosition.Y) - Player.KnownPosition.Y) < 0.001f)
						{
						//	moveVector.Y += 42f;
							Player.Velocity += new Vector3(0f, 2.25f, 0f);// //, 0);
						}
					}
				}

				if (!Player.IsInWater) //Sneaking in water is not a thing.
				{
					if (InputManager.IsDown(InputCommand.MoveDown))
					{
						Player.IsSneaking = true;
					}
					else //if (_prevKeyState.IsKeyDown(KeyBinds.Down))
					{
						Player.IsSneaking = false;
					}
				}
			}

		//	if (moveVector != Vector3.Zero)
			{
				var velocity = moveVector * speedFactor;
				if (Player.IsFlying)
				{
					if ((Player.Velocity * new Vector3(1, 1, 1)).Length() < velocity.Length())
					{
						var old = Player.Velocity;
						Player.Velocity += new Vector3(velocity.X - old.X, velocity.Y - old.Y, velocity.Z - old.Z);
					}
					else
					{
						Player.Velocity = new Vector3(velocity.X, velocity.Y, velocity.Z);
					}
				}
				else
				{
					if ((Player.Velocity * new Vector3(1, 0, 1)).Length() < velocity.Length())
					{
						var old = Player.Velocity;
						Player.Velocity += new Vector3(velocity.X - old.X, 0, velocity.Z - old.Z);
					}
					else
					{
						Player.Velocity = new Vector3(velocity.X, Player.Velocity.Y, velocity.Z);
					}
				}

				//speedFactor *= 20;
				//Player.Velocity += (moveVector * speedFactor);// new Vector3(moveVector.X * speedFactor, moveVector.Y * (speedFactor), moveVector.Z * speedFactor);
			}

		    LastSpeedFactor = speedFactor;

			if (IgnoreNextUpdate)
			{
				IgnoreNextUpdate = false;
			}
			else
			{
				var e = this.GlobalInputManager.CursorInputListener.GetCursorPosition();

				var centerX = Graphics.Viewport.Width / 2;
				var centerY = Graphics.Viewport.Height / 2;

				if (e.X < 10 || e.X > Graphics.Viewport.Width - 10 ||
					e.Y < 10 || e.Y > Graphics.Viewport.Height - 10)
				{
					_previousMousePosition = new Vector2(centerX, centerY);
					Mouse.SetPosition(centerX, centerY);
					IgnoreNextUpdate = true;
				}
				else
				{
					var mouseDelta = _previousMousePosition - e; //this.GlobalInputManager.CursorInputListener.GetCursorPositionDelta();
					var look = new Vector2((-mouseDelta.X), (mouseDelta.Y))
							   * (float)(gt.ElapsedGameTime.TotalSeconds * 30);
					look = -look;

					Player.KnownPosition.HeadYaw -= look.X;
					Player.KnownPosition.Pitch -= look.Y;
					Player.KnownPosition.HeadYaw = MathUtils.NormDeg(Player.KnownPosition.HeadYaw);
					Player.KnownPosition.Pitch = MathHelper.Clamp(Player.KnownPosition.Pitch, -89.9f, 89.9f);

					//Player.KnownPosition.Pitch = MathHelper.Clamp(Player.KnownPosition.Pitch + look.Y, -89.9f, 89.9f);
					// Player.KnownPosition.Yaw = (Player.KnownPosition.Yaw + look.X) % 360f;
					// Player.KnownPosition.Yaw %= 360f;
					_previousMousePosition = e;
				}
			}
		}
    }
}
