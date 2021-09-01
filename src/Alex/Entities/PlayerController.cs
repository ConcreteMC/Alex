using System;
using System.Collections.Generic;
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
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;
using Image = SixLabors.ImageSharp.Image;
using MathF = System.MathF;

namespace Alex.Entities
{
    public class PlayerController : IDisposable
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PlayerController));

		public PlayerIndex PlayerIndex { get; }
		public PlayerInputManager InputManager { get; }
		public MouseInputListener MouseInputListener { get; }

        //public bool IsFreeCam { get; set; }

        private GraphicsDevice Graphics { get; }

        private Player Player { get; }
		private InputManager GlobalInputManager { get; }
		private GamePadInputListener GamePadInputListener { get; }

		private List<InputActionBinding> _inputBindings { get; }

		public PlayerController(GraphicsDevice graphics,
			InputManager inputManager,
			Player player,
			PlayerIndex playerIndex)
		{
			Player = player;
			Graphics = graphics;
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

			optionsProvider.AlexOptions.MouseSensitivity.Bind((value, newValue) => { CursorSensitivity = newValue; });

			GamepadSensitivity = optionsProvider.AlexOptions.ControllerOptions.RightJoystickSensitivity.Value;

			optionsProvider.AlexOptions.ControllerOptions.RightJoystickSensitivity.Bind(
				(value, newValue) => { GamepadSensitivity = newValue; });
			
			_inputBindings = new List<InputActionBinding>(new[]
				{
					InputManager.RegisterListener(
						AlexInputCommand.Jump, InputBindingTrigger.Discrete, CheckMovementPredicate, SetFlying),
					
					InputManager.RegisterListener(AlexInputCommand.MoveUp, InputBindingTrigger.Discrete, 
						CheckMovementPredicate, SetFlying),
					
					InputManager.RegisterListener(
						AlexInputCommand.ToggleCamera, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => Player.Level.Camera.ToggleMode()),
					
					InputManager.RegisterListener(
						AlexInputCommand.DropItem, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => Player.DropHeldItem(InputManager.IsDown(AlexInputCommand.Sprint))),
					
					InputManager.RegisterListener(
						AlexInputCommand.HotBarSelect1, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => { player.Inventory.SelectedSlot = 0; }),
					
					InputManager.RegisterListener(
						AlexInputCommand.HotBarSelect2, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => { player.Inventory.SelectedSlot = 1; }),
					
					InputManager.RegisterListener(
						AlexInputCommand.HotBarSelect3, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => { player.Inventory.SelectedSlot = 2; }),
					
					InputManager.RegisterListener(
						AlexInputCommand.HotBarSelect4, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => { player.Inventory.SelectedSlot = 3; }),
					
					InputManager.RegisterListener(
						AlexInputCommand.HotBarSelect5, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => { player.Inventory.SelectedSlot = 4; }),
					
					InputManager.RegisterListener(
						AlexInputCommand.HotBarSelect6, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => { player.Inventory.SelectedSlot = 5; }),
					
					InputManager.RegisterListener(
						AlexInputCommand.HotBarSelect7, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => { player.Inventory.SelectedSlot = 6; }),
					
					InputManager.RegisterListener(
						AlexInputCommand.HotBarSelect8, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => { player.Inventory.SelectedSlot = 7; }),
					
					InputManager.RegisterListener(
						AlexInputCommand.HotBarSelect9, InputBindingTrigger.Tap, CheckMovementPredicate,
						() => { player.Inventory.SelectedSlot = 8; }),
					
					InputManager.RegisterListener(
						AlexInputCommand.Exit, InputBindingTrigger.Tap,
						() => Alex.Instance.GuiManager.ActiveDialog != null, CloseActiveDialog),
					
					InputManager.RegisterListener(
						AlexInputCommand.ToggleInventory, InputBindingTrigger.Discrete, CanOpenDialog, OpenInventory),
					
					InputManager.RegisterListener(
						AlexInputCommand.ToggleMap, InputBindingTrigger.Discrete, CanOpenDialog, OpenMap),
					
					InputManager.RegisterListener(
						AlexInputCommand.TakeScreenshot, InputBindingTrigger.Discrete, CheckMovementPredicate, TakeScreenshot)
				});
		}

		private void TakeScreenshot()
		{
			Alex.Instance.OnEndDraw += OnEndDraw;
		}

		private void OnEndDraw(object? sender, EventArgs e)
		{
			Alex.Instance.OnEndDraw -= OnEndDraw;
			
			//using (GraphicsContext context = GraphicsContext.CreateContext(Graphics, BlendState.AlphaBlend))
			{
				try
				{
					var w = Graphics.PresentationParameters.BackBufferWidth;
					var h = Graphics.PresentationParameters.BackBufferHeight;
					
					Rgba32[] data = new Rgba32[w * h];
					Graphics.GetBackBufferData(data);

					var pics = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
					var screenshotPath = Path.Combine(pics, $"alex-{DateTime.Now.ToString("s")}.png");
					
					using (Image<Rgba32> t = Image.LoadPixelData(data, w, h))
					{
						using (FileStream fs = File.OpenWrite(screenshotPath))
						{
							t.SaveAsPng(fs, new PngEncoder()
							{
								TransparentColorMode = PngTransparentColorMode.Preserve
							});
						}
					}

					ChatComponent.AddSystemMessage($"{ChatColors.Gray}{ChatFormatting.Italic}Saved screenshot to: {screenshotPath}");
				}
				catch (Exception error)
				{
					Log.Error(error, $"Failed to save screenshot.");

					ChatComponent.AddSystemMessage($"{ChatColors.Red}Failed to save screenshot, see console for more information.");
				}
			}
		}

		private void OpenMap()
		{
			var dialog = new MapDialog(Player.Level.Map);
			Alex.Instance.GuiManager.ShowDialog(dialog);
		}

		private void OpenInventory()
		{
			var dialog = new GuiPlayerInventoryDialog(Player, Player.Inventory);

			if (Player.Network is BedrockClient client)
			{
				dialog.TransactionTracker = client.TransactionTracker;
			}

			//_allowMovementInput = false;H
			    
			Alex.Instance.GuiManager.ShowDialog(dialog);
		}

		private bool CheckMovementPredicate()
		{
			return CheckMovementInput && CanOpenDialog();
		}

		private void SetFlying()
		{
			var now = DateTime.UtcNow;
			var timeBetween = now.Subtract(_lastUp).TotalMilliseconds;

			if (timeBetween <= 350)
			{
				Player.SetFlying(!Player.IsFlying);
			}
		
			_lastUp = now;
		}

		public bool CheckMovementInput { get; set; }

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
		
		private bool IgnoreNextUpdate { get; set; } = false;
	    
		private DateTime _lastForward = DateTime.UtcNow;
		private DateTime _lastUp = DateTime.UtcNow;
		
		private Vector2 _previousMousePosition = Vector2.Zero;

		public void Update(GameTime gameTime)
		{
			UpdatePlayerInput(gameTime);
		}

	    private void UpdatePlayerInput(GameTime gt)
	    {
		    if (CheckInput)
		    {
			    /*if (_allowMovementInput != _previousAllowMovementInput)
			    {
				    CenterCursor();
				    _previousAllowMovementInput = _allowMovementInput;
				    return;
			    }*/

			    UpdateMovementInput(gt);
		    }
		}

	    private bool CanOpenDialog()
	    {
		    if (!(Alex.Instance.GuiManager.FocusManager.FocusedElement is TextInput)
		        && Alex.Instance.GuiManager.ActiveDialog == null)
		    {
			    return true;
		    }

		    return false;
	    }

	    private void CloseActiveDialog()
	    {
		    var activeDialog = Alex.Instance.GuiManager.ActiveDialog;
		    if (activeDialog == null) 
			    return;
		    
		    CenterCursor();
		    Alex.Instance.GuiManager.HideDialog(activeDialog);
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

	    private void UpdateMovementInput(GameTime gt)
	    {
		    if (!CheckMovementInput)
		    {
			    Player.Movement.UpdateHeading(Vector3.Zero);
			    return;
		    }
		    
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

			var moveVector = Vector3.Zero;
			var now = DateTime.UtcNow;

			/*if (InputManager.IsPressed(AlexInputCommand.Jump) || InputManager.IsPressed(InputCommand.MoveUp))
			{
				if (now.Subtract(_lastUp).TotalMilliseconds <= 250)
				{
					Player.SetFlying(!Player.IsFlying);
				}

				_lastUp = now;
			}*/

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
					Player.SetSprinting(true);
				}

				_lastForward = now;
			}
			else
			{
				if (Player.IsSprinting)
					Player.SetSprinting(false);
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
						Player.KnownPosition.Pitch -= look.Y;
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
							- e;

						var look = (new Vector2((-mouseDelta.X), (mouseDelta.Y)) * (float) CursorSensitivity) * (float) (gt.ElapsedGameTime.TotalSeconds);
						Player.KnownPosition.HeadYaw = (Player.KnownPosition.HeadYaw - look.X) % 360f;
						Player.KnownPosition.SetPitchBounded(Player.KnownPosition.Pitch - look.Y);
						_previousMousePosition = e;
					}
				}
			}

			LastVelocity = Player.Velocity;
	    }

	    public bool Disposed { get; private set; } = false;
	    /// <inheritdoc />
	    public void Dispose()
	    {
		    if (Disposed)
			    return;

		    Disposed = true;
		    
		    var bindings = _inputBindings.ToArray();
		    _inputBindings.Clear();

		    foreach (var binding in bindings)
		    {
			    InputManager.UnregisterListener(binding);
		    }
	    }
    }
}
