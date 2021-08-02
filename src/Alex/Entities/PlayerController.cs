using System;
using System.IO;
using System.Linq;
using Alex.Common.Input;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.InGame;
using Alex.Graphics.Camera;
using Alex.Gui.Dialogs;
using Alex.Gui.Dialogs.Containers;
using Alex.Gui.Elements;
using Alex.Net.Bedrock;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Bedrock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using NLog;
using RocketUI;
using RocketUI.Input;
using RocketUI.Input.Listeners;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;
using Image = SixLabors.ImageSharp.Image;
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
		private GamePadInputListener GamePadInputListener { get; }
		
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

			if (InputManager.TryGetListener<GamePadInputListener>(out var gamePadInputListener))
			{
				GamePadInputListener = gamePadInputListener;
			}
			else
			{
				GamePadInputListener = null;
			}
			
			var optionsProvider = Alex.Instance.Services.GetRequiredService<IOptionsProvider>();
			CursorSensitivity = optionsProvider.AlexOptions.MouseSensitivity.Value;

			optionsProvider.AlexOptions.MouseSensitivity.Bind(
				(value, newValue) =>
				{
					CursorSensitivity = newValue;
				});

			GamepadSensitivity = optionsProvider.AlexOptions.ControllerOptions.RightJoystickSensitivity.Value;
			optionsProvider.AlexOptions.ControllerOptions.RightJoystickSensitivity.Bind(
				(value, newValue) =>
				{
					GamepadSensitivity = newValue;
				});

		}

		public bool CheckMovementInput
		{
			get { return _allowMovementInput; }
			set { _allowMovementInput = value; }
		}

		private bool _checkInput = true;

		public bool CheckInput
		{
			get
			{
				return _checkInput;
			}
			set
			{
				if (value != _checkInput)
				{
					IgnoreNextUpdate = true;
					Player.SkipUpdate();
				}
				
				_checkInput = value;
			}
		}
	    private bool _allowMovementInput = true;
	    private bool IgnoreNextUpdate { get; set; } = false;
	    
		private DateTime _lastForward = DateTime.UtcNow;
		private DateTime _lastUp = DateTime.UtcNow;
		
		private Vector2 _previousMousePosition = Vector2.Zero;

		public void Update(GameTime gameTime)
		{
			UpdatePlayerInput(gameTime);
		}

		private bool _previousAllowMovementInput = true;
	    private void UpdatePlayerInput(GameTime gt)
	    {
		    if (CheckInput)
		    {
				CheckGeneralInput(gt);
				
				if (_allowMovementInput != _previousAllowMovementInput)
				{
					CenterCursor();
					_previousAllowMovementInput = _allowMovementInput;
					return;
				}
				UpdateMovementInput(gt);
		    }
		}

	    private void CheckGeneralInput(GameTime gt)
	    {
		    _allowMovementInput = Alex.Instance.GuiManager.ActiveDialog == null;

		    if (_allowMovementInput)
		    {
			    if (InputManager.IsPressed(AlexInputCommand.HotBarSelectPrevious)
			        || MouseInputListener.IsButtonDown(MouseButton.ScrollUp))
			    {
				    Player.Inventory.SelectedSlot--;
			    }
			    else if (InputManager.IsPressed(AlexInputCommand.HotBarSelectNext)
			             || MouseInputListener.IsButtonDown(MouseButton.ScrollDown))
			    {
				    Player.Inventory.SelectedSlot++;
			    }

			    if (InputManager.IsPressed(AlexInputCommand.HotBarSelect1)) Player.Inventory.SelectedSlot = 0;
			    if (InputManager.IsPressed(AlexInputCommand.HotBarSelect2)) Player.Inventory.SelectedSlot = 1;
			    if (InputManager.IsPressed(AlexInputCommand.HotBarSelect3)) Player.Inventory.SelectedSlot = 2;
			    if (InputManager.IsPressed(AlexInputCommand.HotBarSelect4)) Player.Inventory.SelectedSlot = 3;
			    if (InputManager.IsPressed(AlexInputCommand.HotBarSelect5)) Player.Inventory.SelectedSlot = 4;
			    if (InputManager.IsPressed(AlexInputCommand.HotBarSelect6)) Player.Inventory.SelectedSlot = 5;
			    if (InputManager.IsPressed(AlexInputCommand.HotBarSelect7)) Player.Inventory.SelectedSlot = 6;
			    if (InputManager.IsPressed(AlexInputCommand.HotBarSelect8)) Player.Inventory.SelectedSlot = 7;
			    if (InputManager.IsPressed(AlexInputCommand.HotBarSelect9)) Player.Inventory.SelectedSlot = 8;

			    if (InputManager.IsPressed(AlexInputCommand.ToggleCamera))
			    {
				    World.Camera.ToggleMode();
			    }

			    if (InputManager.IsPressed(AlexInputCommand.DropItem))
			    {
				    //Sprint is bound to LeftCtrl by default.
				    Player.DropHeldItem(InputManager.IsDown(AlexInputCommand.Sprint));
			    }

			    if (InputManager.IsPressed(AlexInputCommand.TakeScreenshot))
			    {
				    //Take screenshot.
				    Alex.Instance.UiTaskManager.Enqueue(
					    () =>
					    {
						    var blendMode = Graphics.BlendState;

						    try
						    {
							    Graphics.BlendState = BlendState.NonPremultiplied;

							    var graphicsDevice = Alex.Instance.GraphicsDevice;
							    //	var viewPort = Alex.Instance.DeviceManager.PreferredBackBufferWidth
							    var w = graphicsDevice.PresentationParameters.BackBufferWidth;
							    var h = graphicsDevice.PresentationParameters.BackBufferHeight;
							    Color[] data = new Color[w * h];

							    graphicsDevice.GetBackBufferData(data);

							    Image<Rgba32> t = Image.LoadPixelData(
								    data.Select(x => new Rgba32(x.PackedValue)).ToArray(), w, h);

							    //Texture2D t = new Texture2D(graphicsDevice, w, h, false, SurfaceFormat.Color);
							    //t.SetData(data);
							    var pics = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
							    var screenshotPath = Path.Combine(pics, $"alex-{DateTime.Now.ToString("s")}.png");

							    using (FileStream fs = File.OpenWrite(screenshotPath))
							    {
								    t.SaveAsPng(fs);
							    }

							    t.Dispose();

							    ChatComponent.AddSystemMessage(
								    $"{ChatColors.Gray}{ChatFormatting.Italic}Saved screenshot to: {screenshotPath}");
						    }
						    catch (Exception error)
						    {
							    Log.Error(error, $"Failed to save screenshot.");

							    ChatComponent.AddSystemMessage(
								    $"{ChatColors.Red}Failed to save screenshot, see console for more information.");
						    }
						    finally
						    {
							    Graphics.BlendState = blendMode;
						    }
					    });
			    }
		    }

		    if (InputManager.IsPressed(AlexInputCommand.Exit))
		    {
			    CloseActiveDialog();
		    }
		    else if (InputManager.IsPressed(AlexInputCommand.ToggleInventory) && CanOpenDialog())
		    {
			    var dialog = new GuiPlayerInventoryDialog(Player, Player.Inventory);

			    if (Player.Network is BedrockClient client)
			    {
				    dialog.TransactionTracker = client.TransactionTracker;
			    }

			    //_allowMovementInput = false;H
			    
			    Alex.Instance.GuiManager.ShowDialog(dialog);
		    }
		    else if (InputManager.IsPressed(AlexInputCommand.ToggleMap) && CanOpenDialog())
		    {
			    var dialog = new MapDialog(Player.Level.Map);
			    Alex.Instance.GuiManager.ShowDialog(dialog);
		    }

		    _allowMovementInput = Alex.Instance.GuiManager.ActiveDialog == null;
	    }

	    private bool CanOpenDialog()
	    {
		    return !(Alex.Instance.GuiManager.FocusManager.FocusedElement is TextInput) && (!CloseActiveDialog());
	    }

	    private bool CloseActiveDialog()
	    {
		    var activeDialog = Alex.Instance.GuiManager.ActiveDialog;
		    if (activeDialog == null) 
			    return false;
		    
		    CenterCursor();
		    //_allowMovementInput = true;
		    Alex.Instance.GuiManager.HideDialog(activeDialog);

		    return true;

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
	    private double CursorSensitivity { get; set; } = 30d;
	    private double GamepadSensitivity { get; set; } = 200d;
	    private bool _jumping = false;

	    private void SetSprinting(bool sprinting)
	    {
		    if (sprinting)
		    {
			    if (!Player.IsSprinting && Player.CanSprint)
			    {
				  //  Player.Network?.EntityAction((int) Player.EntityId, EntityAction.StartSprinting);
				    Player.IsSprinting = true;
			    }
		    }
		    else
		    {
			    if (Player.IsSprinting)
			    {
				    Player.IsSprinting = false;
				  //  Player.Network?.EntityAction((int) Player.EntityId, EntityAction.StopSprinting);
			    }
		    }
	    }
	    
	    private void UpdateMovementInput(GameTime gt)
	    {
		    if (!_allowMovementInput)
		    {
			    Player.Movement.UpdateHeading(Vector3.Zero);
			    return;
		    }

			var moveVector = Vector3.Zero;
			var now = DateTime.UtcNow;

			if (Player.CanFly)
			{
			    if (InputManager.IsDown(AlexInputCommand.MoveUp) || InputManager.IsDown(AlexInputCommand.Jump))
			    {
				    if ((InputManager.IsBeginPress(AlexInputCommand.MoveUp) || InputManager.IsBeginPress(AlexInputCommand.Jump)) &&
				        now.Subtract(_lastUp).TotalMilliseconds <= 125)
				    {
					    Player.IsFlying = !Player.IsFlying;
				    }

				    _lastUp = now;
			    }
		    }

		    //float speedFactor = (float)Player.CalculateMovementSpeed();

		    bool holdingDownSprint = InputManager.IsDown(AlexInputCommand.Sprint);

		    bool canSwim = Player.CanSwim && Player.FeetInWater && Player.HeadInWater;

		    if (canSwim)
		    {
			    if (InputManager.IsBeginPress(AlexInputCommand.Sprint))
			    {
				    Player.IsSwimming = !Player.IsSwimming;
			    }
		    }
		    else if (Player.IsSwimming)
		    {
			    Player.IsSwimming = false;
		    }

		    if (InputManager.IsDown(AlexInputCommand.MoveForwards))
			{
				moveVector.Z += 1;

				if (holdingDownSprint || (InputManager.IsBeginPress(AlexInputCommand.MoveForwards)
				                      && now.Subtract(_lastForward).TotalMilliseconds <= 125))
				{
					SetSprinting(true);
				}

				_lastForward = now;
			}
			else
			{
				if (Player.IsSprinting)
					SetSprinting(false);
			}

			if (InputManager.IsDown(AlexInputCommand.MoveBackwards))
				moveVector.Z -= 1;

			if (InputManager.IsDown(AlexInputCommand.MoveLeft))
				moveVector.X += 1;

			if (InputManager.IsDown(AlexInputCommand.MoveRight))
				moveVector.X -= 1;
			
			if (Player.IsFlying)
			{
				//speedFactor *= 1f + (float)Player.FlyingSpeed;
				//speedFactor *= 2.5f;

				if (InputManager.IsDown(AlexInputCommand.MoveUp))
					moveVector.Y += 1;

				if (InputManager.IsDown(AlexInputCommand.MoveDown))
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
				if (_jumping && Player.Velocity.Y <= 0.00001f)
					_jumping = false;
				
				var jumpPressed = (InputManager.IsDown(AlexInputCommand.Jump)
				                   || InputManager.IsDown(AlexInputCommand.MoveUp));
				
				bool readyToJump = Player.Velocity.Y <= 0.00001f && Player.Velocity.Y >= -0.00001f && Math.Abs(LastVelocity.Y - Player.Velocity.Y) < 0.0001f;
				
				if (jumpPressed)
				{
					if (Player.IsInWater && !_jumping)
					{
						_jumping = true;
						Player.Jump();
					}
					else if (!Player.IsInWater && Player.KnownPosition.OnGround && readyToJump)
					{
						//	moveVector.Y += 42f;
						//	Player.Velocity += new Vector3(0f, 4.65f, 0f); // //, 0);
						Player.Jump();
					}
				}

				if (!Player.IsInWater) //Sneaking in water is not a thing.
				{
					if (InputManager.IsDown(AlexInputCommand.MoveDown) || InputManager.IsDown(AlexInputCommand.Sneak))
					{
						Player.IsSneaking = true;
					}
					else //if (_prevKeyState.IsKeyDown(KeyBinds.Down))
					{
						Player.IsSneaking = false;
					}
				}
			}

			if (Player.IsSwimming && moveVector.LengthSquared() <= 0.01f)
				Player.IsSwimming = false;
			
			Player.Movement.UpdateHeading(moveVector);

				// LastSpeedFactor = speedFactor;
		    if (IgnoreNextUpdate)
			{
				IgnoreNextUpdate = false;
			}
			else
			{
				var checkMouseInput = true;
				if (GamePadInputListener != null && GamePadInputListener.IsConnected)
				{
					var inputValue = GamePadInputListener.GetCursorPosition();

					if (inputValue != Vector2.Zero)
					{
						checkMouseInput = false;
						
						var look = (new Vector2((inputValue.X), (inputValue.Y)) * (float) GamepadSensitivity)
						                                                       * (float) (gt.ElapsedGameTime.TotalSeconds);

						look = -look;
						
						Player.KnownPosition.HeadYaw = (Player.KnownPosition.HeadYaw - look.X) % 360f;
						//Player.KnownPosition.HeadYaw -= look.X;
						
						Player.KnownPosition.Pitch -= look.Y;
						//Player.KnownPosition.HeadYaw = MathUtils.NormDeg(Player.KnownPosition.HeadYaw);
						//Player.KnownPosition.Pitch = MathHelper.Clamp(Player.KnownPosition.Pitch, -89.9f, 89.9f);
					}
				}

				if (checkMouseInput)
				{
					var e = MouseInputListener.GetCursorPosition();
					
					if (e.X < 10 || e.X > Graphics.Viewport.Width - 10 || e.Y < 10
					    || e.Y > Graphics.Viewport.Height - 10)
					{
						CenterCursor();
						IgnoreNextUpdate = true;
					}
					else
					{
						var mouseDelta =
							_previousMousePosition
							- e; //this.GlobalInputManager.CursorInputListener.GetCursorPositionDelta();

						var look = (new Vector2((-mouseDelta.X), (mouseDelta.Y)) * (float) CursorSensitivity) * (float) (gt.ElapsedGameTime.TotalSeconds);

						Player.KnownPosition.HeadYaw = (Player.KnownPosition.HeadYaw - look.X) % 360f;

						Player.KnownPosition.SetPitchBounded(Player.KnownPosition.Pitch - look.Y);
						//Player.KnownPosition.Pitch -= look.Y; 
						
						// MathHelper.Clamp(Player.KnownPosition.Pitch - look.Y, 0f, 180f);
						//Player.KnownPosition.Pitch = MathHelper.Clamp(Player.KnownPosition.Pitch, -89.9f, 89.9f);
						//Player.KnownPosition.HeadYaw = MathUtils.NormDeg(Player.KnownPosition.HeadYaw);
						//Player.KnownPosition.Pitch = MathHelper.Clamp(Player.KnownPosition.Pitch, -89.9f, 89.9f);

						//Player.KnownPosition.Pitch = MathHelper.Clamp(Player.KnownPosition.Pitch + look.Y, -89.9f, 89.9f);
						// Player.KnownPosition.Yaw = (Player.KnownPosition.Yaw + look.X) % 360f;
						// Player.KnownPosition.Yaw %= 360f;
						_previousMousePosition = e;
					}
				}
			}

			LastVelocity = Player.Velocity;
	    }
	    
	    float FixValue(float value)
	    {
		    var val = value;

		    if (val < 0f)
			    val = 360f - (MathF.Abs(val) % 360f);
		    else if (val > 360f)
			    val = val % 360f;

		    return val;
	    }
    }
}
