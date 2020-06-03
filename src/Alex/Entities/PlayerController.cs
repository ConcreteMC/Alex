using System;
using Alex.API.Input;
using Alex.API.Input.Listeners;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Gui.Dialogs.Containers;
using Alex.Worlds;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using MathF = System.MathF;

namespace Alex.Entities
{
    public class PlayerController
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PlayerController));

		public PlayerIndex PlayerIndex { get; }
		public PlayerInputManager InputManager { get; }
		public MouseInputListener MouseInputListener { get; }

        //public bool IsFreeCam { get; set; }

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

          //  IsFreeCam = true;

			GlobalInputManager = inputManager;
			InputManager = inputManager.GetOrAddPlayerManager(playerIndex);
			InputManager.AddListener(MouseInputListener = new MouseInputListener(playerIndex));

			var optionsProvider = Alex.Instance.Services.GetService<IOptionsProvider>();
			CursorSensitivity = optionsProvider.AlexOptions.MouseSensitivity.Value;

			optionsProvider.AlexOptions.MouseSensitivity.Bind(
				(value, newValue) =>
				{
					CursorSensitivity = newValue;
				});
		}

		private bool _inActive = true;

		public bool CheckMovementInput
		{
			get { return _allowMovementInput; }
			set { _allowMovementInput = value; }
		}

		public bool CheckInput { get; set; } = true;
	    private bool _allowMovementInput = true;
	    private bool IgnoreNextUpdate { get; set; } = false;
		private DateTime _lastForward = DateTime.UtcNow;
		private DateTime _lastJump = DateTime.UtcNow;
		private DateTime _lastUp = DateTime.UtcNow;
		
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
				UpdateMovementInput(gt);
		    }
		    else if (!_inActive)
		    {
			    _inActive = true;
		    }
		}

	    private void CheckGeneralInput(GameTime gt)
	    {
		    if (World.FormManager.IsShowingForm)
			    return;
		    
		    if (MouseInputListener.IsButtonDown(MouseButton.ScrollUp))
		    {
			    Player.Inventory.SelectedSlot--;
		    }

		    if (MouseInputListener.IsButtonDown(MouseButton.ScrollDown))
		    {
			    Player.Inventory.SelectedSlot++;
		    }
		    
		    if (InputManager.IsPressed(InputCommand.HotBarSelect1)) Player.Inventory.SelectedSlot = 0;
		    if (InputManager.IsPressed(InputCommand.HotBarSelect2)) Player.Inventory.SelectedSlot = 1;
		    if (InputManager.IsPressed(InputCommand.HotBarSelect3)) Player.Inventory.SelectedSlot = 2;
		    if (InputManager.IsPressed(InputCommand.HotBarSelect4)) Player.Inventory.SelectedSlot = 3;
		    if (InputManager.IsPressed(InputCommand.HotBarSelect5)) Player.Inventory.SelectedSlot = 4;
		    if (InputManager.IsPressed(InputCommand.HotBarSelect6)) Player.Inventory.SelectedSlot = 5;
		    if (InputManager.IsPressed(InputCommand.HotBarSelect7)) Player.Inventory.SelectedSlot = 6;
		    if (InputManager.IsPressed(InputCommand.HotBarSelect8)) Player.Inventory.SelectedSlot = 7;
		    if (InputManager.IsPressed(InputCommand.HotBarSelect9)) Player.Inventory.SelectedSlot = 8;
		    
		    if (InputManager.IsPressed(InputCommand.Exit))
		    {
			    var activeDialog = Alex.Instance.GuiManager.ActiveDialog;

			    if (activeDialog != null)
			    {
				    CenterCursor();
				    Alex.Instance.GuiManager.HideDialog(activeDialog);
			    }
			    
			    if (activeDialog is GuiPlayerInventoryDialog)
				    _guiPlayerInventoryDialog = null;
		    }
			else if (InputManager.IsPressed(InputCommand.ToggleInventory))
			{
				if (_guiPlayerInventoryDialog == null)
				{
					//_allowMovementInput = false;
					Alex.Instance.GuiManager.ShowDialog(_guiPlayerInventoryDialog = new GuiPlayerInventoryDialog(Player, Player.Inventory));
				}
				else
				{
					CenterCursor();
					//_allowMovementInput = true;
					Alex.Instance.GuiManager.HideDialog(_guiPlayerInventoryDialog);
					_guiPlayerInventoryDialog = null;
				}
			}

		    _allowMovementInput = Alex.Instance.GuiManager.ActiveDialog == null;
	    }

	    private void CenterCursor()
	    {
		    var centerX = Graphics.Viewport.Width / 2;
		    var centerY = Graphics.Viewport.Height / 2;
		    
		    Mouse.SetPosition(centerX, centerY);
		    
		    _previousMousePosition = new Vector2(centerX, centerY);
		    IgnoreNextUpdate = true;
	    }

	    public float LastSpeedFactor = 0f;
	    private Vector3 LastVelocity { get; set; } = Vector3.Zero;
	    private bool WasInWater { get; set; } = false;
	    private double CursorSensitivity { get; set; } = 30;
	    private void UpdateMovementInput(GameTime gt)
	    {
		    if (!_allowMovementInput) return;

			var moveVector = Vector3.Zero;
			var now = DateTime.UtcNow;

			if (Player.CanFly)
			{
			    if (InputManager.IsPressed(InputCommand.ToggleCameraFree))
			    {
				    Player.IsFlying = !Player.IsFlying;
			    }
			    else if (InputManager.IsDown(InputCommand.MoveUp))
			    {
				    if (InputManager.IsBeginPress(InputCommand.MoveUp) &&
				        now.Subtract(_lastUp).TotalMilliseconds <= 125)
				    {
					    Player.IsFlying = !Player.IsFlying;
				    }

				    _lastUp = now;
			    }
		    }

		    float modifier = Player.MovementSpeedModifier;

			if (Player.IsInWater || (WasInWater && Player.AboveWater))
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
				if (!Player.IsSprinting && Player.CanSprint)
				{
					if (InputManager.IsBeginPress(InputCommand.MoveForwards) &&
						now.Subtract(_lastForward).TotalMilliseconds <= 125)
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
				if (Player.FeetInWater && InputManager.IsDown(InputCommand.MoveUp))
				{
					Player.Velocity = new Vector3(Player.Velocity.X, 1f * speedFactor, Player.Velocity.Z);
				}
				else if (!Player.IsInWater && Player.KnownPosition.OnGround && InputManager.IsDown(InputCommand.MoveUp))
				{
					if (Player.Velocity.Y <= 0.00001f && Player.Velocity.Y >= -0.00001f
					    && Math.Abs(LastVelocity.Y - Player.Velocity.Y) < 0.0001f)
					{
						//	moveVector.Y += 42f;
						//	Player.Velocity += new Vector3(0f, 4.65f, 0f); // //, 0);
						Player.Jump();
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

			WasInWater = Player.FeetInWater;
			
		//	if (moveVector != Vector3.Zero)
			{
				var velocity = moveVector * speedFactor;
				velocity = Vector3.Transform(velocity,
					Matrix.CreateRotationY(-MathHelper.ToRadians(Player.KnownPosition.HeadYaw)));
				
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
					var old = Player.Velocity;
					var oldLength = (Player.Velocity * new Vector3(1, 0, 1)).Length();
					if (oldLength < velocity.Length())
					{
						Player.Velocity += new Vector3(velocity.X - old.X, 0, velocity.Z - old.Z);
					}
					else
					{
						
						Player.Velocity = new Vector3(MathF.Abs(old.X) < 0.0001f ? velocity.X : old.X, Player.Velocity.Y, MathF.Abs(old.Z) < 0.0001f ? velocity.Z : old.Z);
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
							   * (float)(gt.ElapsedGameTime.TotalSeconds * CursorSensitivity);
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

			LastVelocity = Player.Velocity;
	    }
    }
}
