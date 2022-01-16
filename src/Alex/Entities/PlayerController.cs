using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Alex.Common.Input;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.InGame;
using Alex.Graphics.Camera;
using Alex.Gui.Dialogs;
using Alex.Gui.Dialogs.Containers;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Hud;
using Alex.Net.Bedrock;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Bedrock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET.Net;
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

		private bool _invertX, _invertY;
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

			var optionsProvider = Alex.Instance.Options;
			CursorSensitivity = optionsProvider.AlexOptions.MouseSensitivity.Value;

			_invertX = optionsProvider.AlexOptions.ControllerOptions.InvertX.Value;
			_invertY = optionsProvider.AlexOptions.ControllerOptions.InvertY.Value;

			optionsProvider.AlexOptions.ControllerOptions.InvertX.Bind(
				(value, newValue) =>
				{
					_invertX = newValue;
				});
			
			optionsProvider.AlexOptions.ControllerOptions.InvertY.Bind(
				(value, newValue) =>
				{
					_invertY = newValue;
				});
			
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
						AlexInputCommand.Exit, InputBindingTrigger.Discrete, CloseActiveDialog),

					InputManager.RegisterListener(
						AlexInputCommand.ToggleInventory, InputBindingTrigger.Discrete, CanOpenDialog, OpenInventory),
					
					InputManager.RegisterListener(
						AlexInputCommand.ToggleMap, InputBindingTrigger.Discrete, CanOpenDialog, OpenMap),
					
					InputManager.RegisterListener(
						AlexInputCommand.TakeScreenshot, InputBindingTrigger.Discrete, CheckMovementPredicate, TakeScreenshot),
					
					InputManager.RegisterListener(AlexInputCommand.ToggleDebugInfo, InputBindingTrigger.Discrete, CanOpenDialog, ToggleDebugInfo),
					InputManager.RegisterListener(AlexInputCommand.ToggleBoundingboxDebugInfo, InputBindingTrigger.Discrete, CanOpenDialog, ToggleBoundingBoxes),
					InputManager.RegisterListener(AlexInputCommand.ToggleNetworkDebugInfo, InputBindingTrigger.Discrete, CanOpenDialog, ToggleNetworkDebugInfo),
					InputManager.RegisterListener(AlexInputCommand.ToggleFog, InputBindingTrigger.Discrete, CanOpenDialog, ToggleFog),
					InputManager.RegisterListener(AlexInputCommand.ToggleWireframe, InputBindingTrigger.Discrete, CanOpenDialog, ToggleWireframe)
				});
		}

		private void ToggleWireframe()
		{
			Player.Level.ToggleWireFrame();
		}

		private void ToggleFog()
		{
			//Player.Level.ChunkManager.FogEnabled = !Player.Level.ChunkManager.FogEnabled;
		}
		
		private void ToggleBoundingBoxes()
		{
			Player.Level.RenderBoundingBoxes = !Player.Level.RenderBoundingBoxes;
		}
		
		private void ToggleDebugInfo()
		{
			var activeState = Alex.Instance.GameStateManager.GetActiveState();
			if (activeState is PlayingState ps)
			{
				ps.RenderDebug = !ps.RenderDebug;
			}
			else
			{
				Log.Warn($"Active state was not playstate, got {activeState?.GetType().ToString()} instead!");
			}
		}
		
		private void ToggleNetworkDebugInfo()
		{
			var activeState = Alex.Instance.GameStateManager.GetActiveState();
			if (activeState is PlayingState ps)
			{
				ps.RenderNetworking = !ps.RenderNetworking;
			}
			else
			{
				Log.Warn($"Active state was not playstate, got {activeState?.GetType().ToString()} instead!");
			}
		}
		
		private void TakeScreenshot()
		{
			Alex.Instance.OnEndDraw += OnEndDraw;
		}

		private void OnEndDraw(object? sender, EventArgs e)
		{
			Alex.Instance.OnEndDraw -= OnEndDraw;
			var w = Graphics.PresentationParameters.BackBufferWidth;
			var h = Graphics.PresentationParameters.BackBufferHeight;

			Rgba32[] data = new Rgba32[w * h];

			try
			{
				Graphics.GetBackBufferData(data);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed to save screenshot.");

				ChatComponent.AddSystemMessage(
					$"{ChatColors.Red}Failed to save screenshot, see console for more information.");

				return;
			}

			ThreadPool.QueueUserWorkItem(
				o =>
				{
					try
					{


						var pics = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
						var screenshotPath = Path.Combine(pics, $"alex-{DateTime.Now.ToString("s")}.png");

						using (Image<Rgba32> t = Image.LoadPixelData(data, w, h))
						{
							using (FileStream fs = File.OpenWrite(screenshotPath))
							{
								t.SaveAsPng(
									fs, new PngEncoder() { TransparentColorMode = PngTransparentColorMode.Preserve });
							}
						}

						ChatComponent.AddSystemMessage(
							$"{ChatColors.Gray}{ChatFormatting.Italic}Saved screenshot to: {screenshotPath}");
					}
					catch (Exception error)
					{
						Log.Error(error, $"Failed to save screenshot.");

						ChatComponent.AddSystemMessage(
							$"{ChatColors.Red}Failed to save screenshot, see console for more information.");
					}
				});
		}

		private void OpenMap()
		{
			var dialog = new MapDialog(Player.Level.Map);
			dialog.GuiManager = Alex.Instance.GuiManager;
			
			dialog.Show();
		}

		private void OpenInventory()
		{
			var dialog = new GuiPlayerInventoryDialog(Player, Player.Inventory);
			dialog.GuiManager = Alex.Instance.GuiManager;
			
			if (Player.Network is BedrockClient client)
			{
				dialog.TransactionTracker = client.TransactionTracker;
			}

			//_allowMovementInput = false;H
			dialog.Show();
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
	    
		private long _lastForward = 0;
		private DateTime _lastUp = DateTime.UtcNow;
		
		private Vector2 _previousMousePosition = Vector2.Zero;

		public void Update(GameTime gameTime)
		{
			UpdatePlayerInput(gameTime);

			bool isShown = Alex.Instance.IsMouseVisible;
			
			bool showCursor = false;
			if (Alex.Instance.IsActive)
			{
				bool hasActiveDialog = GetActiveDialog(out _);

				if (hasActiveDialog || !CanOpenDialog())
				{
					showCursor = true;
				}
			}
			else
			{
				showCursor = true;
			}

			if (showCursor != isShown)
			{
				Alex.Instance.IsMouseVisible = showCursor;
			}
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
			    UpdateMouseInput(gt);
		    }
		    else
		    {
			    Player.Movement.UpdateHeading(Vector3.Zero);
		    }
		}

	    private bool GetActiveDialog(out DialogBase dialog)
	    {
		    dialog = Alex.Instance.GuiManager.ActiveDialog;
		    return dialog != null && Alex.Instance.GameStateManager.GetActiveState() is PlayingState;
	    }

	    private bool CanOpenDialog()
	    {
		    var focusedElement = Alex.Instance.GuiManager.FocusManager.FocusedElement;

		    if ((focusedElement == null || !focusedElement.CanFocus || !focusedElement.Enabled
		         || !focusedElement.Focused) && Alex.Instance.GuiManager.ActiveDialog == null
		                                     && Alex.Instance.GameStateManager.GetActiveState() is PlayingState)
		    {
			    return true;
		    }

		    return false;
	    }

	    private void CloseActiveDialog()
	    {
		    if (GetActiveDialog(out var dialog))
		    {
			    if (dialog == null)
				    return;

			    CenterCursor();
			    dialog.Close();
			    Player.SkipUpdate();
		    }
		    else if (CanOpenDialog())
		    {
			    Alex.Instance.GameStateManager.SetActiveState<InGameMenuState>(true, false);
			    Player.SkipUpdate();
		    }
	    }

	    private void CenterCursor()
	    {
		    var centerX = Graphics.Viewport.Width / 2;
		    var centerY = Graphics.Viewport.Height / 2;
		    
		    Mouse.SetPosition(centerX, centerY);
		    
		    _previousMousePosition = new Vector2(centerX, centerY);
		    IgnoreNextUpdate = true;
	    }
	    
	    private Vector3 LastVelocity { get; set; } = Vector3.Zero;
	    private double CursorSensitivity { get; set; } = 30d;
	    private double GamepadSensitivity { get; set; } = 200d;
	    private bool _jumping = false;

	    private MouseState _mouseState = new MouseState();
	    private void UpdateMovementInput(GameTime gt)
	    {
		    if (!CheckMovementInput)
		    {
			    Player.Movement.UpdateHeading(Vector3.Zero);
			    //InputFlags = 0;
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
		    
		      var inputFlags = GetInputFlags();

			bool canSwim = Player.CanSwim && Player.FeetInWater && Player.HeadInWater;

			bool swimming = Player.IsSwimming;
			bool sprinting = Player.IsSprinting;

			if (!sprinting && (inputFlags & AuthInputFlags.Sprinting) != 0)
			{
				sprinting = true;
			}
			else if (sprinting && (inputFlags & AuthInputFlags.Sprinting) == 0)
			{
				sprinting = false;
			}

			if ((inputFlags & AuthInputFlags.StartSprinting) != 0)
		    {
			    if (canSwim)
			    {
				    swimming = true;
				    sprinting = false;
			    }
			    else
			    {
				    sprinting = true;
				    swimming = false;
			    }
		    }
		    else if ((inputFlags & AuthInputFlags.StopSprinting) != 0)
		    {
			    swimming = false;
			    sprinting = false;
		    }

			if (_jumping && Player.Velocity.Y <= 0.00001f && Player.FeetInWater)
			    _jumping = false;

		    if (!_canJump && Player.KnownPosition.OnGround && Math.Abs(LastVelocity.Y - Player.Velocity.Y)
		        < 0.0001f)
		    {
			    _canJump = true;
		    }

		    if (!Player.IsFlying && ((inputFlags & AuthInputFlags.JumpDown) != 0 || (inputFlags & AuthInputFlags.WantUp) != 0))
		    {
			    if (Player.IsInWater && !_jumping)
			    {
				    _jumping = true;
				    inputFlags |= AuthInputFlags.StartJumping;
				    Player.Jump();
			    }
			    else if (!Player.IsInWater && _canJump)
			    {
				    bool readyToJump = Player.Velocity.Y <= 0.00001f && Player.Velocity.Y >= -0.00001f
				                                                     && Math.Abs(LastVelocity.Y - Player.Velocity.Y)
				                                                     < 0.0001f;

				    if (Player.KnownPosition.OnGround && readyToJump)
				    {
					    inputFlags |= AuthInputFlags.StartJumping;
					    _canJump = false;
					    Player.Jump();
				    }
			    }
		    }

		    bool wasSneaking = Player.IsSneaking;
		    Player.IsSneaking = !Player.IsInWater && (inputFlags & AuthInputFlags.Sneaking) != 0;

		    if (Player.IsSneaking != wasSneaking)
		    {
			    if (Player.IsSneaking)
			    {
				    inputFlags |= AuthInputFlags.StartSneaking;
			    }
			    else
			    {
				    inputFlags |= AuthInputFlags.StopSneaking;
			    }
		    }
		    
		    bool wasSwimming = Player.IsSwimming;
		    Player.SetSwimming(swimming && canSwim);
		    if (Player.IsSwimming != wasSwimming)
		    {
			    if (Player.IsSwimming)
			    {
				    inputFlags |= AuthInputFlags.StartSwimming;
			    }
			    else
			    {
				    inputFlags |= AuthInputFlags.StopSwimming;
			    }
		    }
		    
		    bool wasSprinting = Player.IsSprinting;
		    Player.SetSprinting(sprinting && !Player.IsSwimming && !Player.IsInWater && Player.CanSprint);

		    if (Player.IsSprinting != wasSprinting)
		    {
			    if (Player.IsSprinting)
			    {
				    inputFlags |= AuthInputFlags.StartSprinting;
			    }
			    else
			    {
				    inputFlags |= AuthInputFlags.StopSprinting;
			    }
		    }

		    InputFlags = inputFlags;
		    Player.Movement.UpdateHeading(GetMoveVector(inputFlags));
		    
			LastVelocity = Player.Velocity;
	    }

	    public Vector3 GetMoveVector(AuthInputFlags inputFlags)
	    {
		    var moveVector = Vector3.Zero;

		    if ((inputFlags & AuthInputFlags.WalkForwards) != 0)
			    moveVector.Z += 1;
		    
		    if ((inputFlags & AuthInputFlags.WalkBackwards) != 0)
			    moveVector.Z -= 1;

		    if ((inputFlags & AuthInputFlags.StrafeLeft) != 0)
			    moveVector.X += 1;

		    if ((inputFlags & AuthInputFlags.StrafeRight) != 0)
			    moveVector.X -= 1;
		    
		    if (Player.IsFlying)
		    {
			    if ((inputFlags & AuthInputFlags.WantUp) != 0)
				    moveVector.Y += 1;

			    if ((inputFlags & AuthInputFlags.WantDown) != 0)
				    moveVector.Y -= 1;
		    }
		    
		    return moveVector;
	    }
	    
	    public AuthInputFlags InputFlags { get; private set; }

	    private bool _canJump = true;
	    private AuthInputFlags GetInputFlags()
	    {
		    AuthInputFlags inputFlags = 0;

		    var previousInputFlags = InputFlags;

		    if (InputManager.IsDown(AlexInputCommand.MoveForwards))
			    inputFlags |= AuthInputFlags.WalkForwards;

		    if (InputManager.IsDown(AlexInputCommand.MoveBackwards))
			    inputFlags |= AuthInputFlags.WalkBackwards;

		    if (InputManager.IsDown(AlexInputCommand.MoveLeft))
			    inputFlags |= AuthInputFlags.StrafeLeft;

		    if (InputManager.IsDown(AlexInputCommand.MoveRight))
			    inputFlags |= AuthInputFlags.StrafeRight;

		    if (InputManager.IsDown(AlexInputCommand.MoveUp))
		    {
			    inputFlags |= AuthInputFlags.Ascend;
			    inputFlags |= AuthInputFlags.WantUp;
		    }

		    if (InputManager.IsDown(AlexInputCommand.MoveDown))
		    {
			    inputFlags |= AuthInputFlags.Descend;
			    inputFlags |= AuthInputFlags.WantDown;
			    inputFlags |= AuthInputFlags.Sneaking;
		    }

		    if (InputManager.IsDown(AlexInputCommand.Sneak))
		    {
			    inputFlags |= AuthInputFlags.SneakDown;
			    inputFlags |= AuthInputFlags.Sneaking;

			    inputFlags |= AuthInputFlags.WantDown;
		    }

		    if (InputManager.IsPressed(AlexInputCommand.Sneak))
		    {
			    if (Player.IsSneaking)
			    {
				    inputFlags |= AuthInputFlags.StopSneaking;
			    }
			    else
			    {
				    inputFlags |= AuthInputFlags.StartSneaking;
			    }
		    }

		    if ((inputFlags & AuthInputFlags.WalkForwards) != 0)
		    {
			    bool pressedWalk = (inputFlags & AuthInputFlags.WalkForwards) != 0
			                       && (previousInputFlags & AuthInputFlags.WalkForwards) == 0;

			    if (Player.IsSprinting || (pressedWalk && Player.Age - _lastForward <= 3))
			    {
				    inputFlags |= AuthInputFlags.Sprinting;
			    }

			    //if (pressedWalk)
			    {
				    _lastForward = Player.Age;
			    }
		    }

		    if (InputManager.IsDown(AlexInputCommand.Sprint))
		    {
			    inputFlags |= AuthInputFlags.SprintDown;
			    inputFlags |= AuthInputFlags.Sprinting;
		    }

		    if (InputManager.IsPressed(AlexInputCommand.SprintToggle))
		    {
			    if (Player.IsSprinting)
			    {
				    inputFlags |= AuthInputFlags.StopSprinting;
			    }
			    else
			    {
				    inputFlags |= AuthInputFlags.StartSprinting;
			    }
		    }

		    if (InputManager.IsDown(AlexInputCommand.Jump))
		    {
			    inputFlags |= AuthInputFlags.JumpDown;
			    inputFlags |= AuthInputFlags.WantUp;

			    if (!Player.IsFlying)
				    inputFlags |= AuthInputFlags.NorthJump;
		    }

		    return inputFlags;
	    }

	    public void Tick()
	    {
		    if (!CheckMovementInput)
			    return;
		    
		    /*InputFlags = GetInputFlags();
		    Player.Movement.UpdateHeading(GetMoveVector(InputFlags));

		    if ((InputFlags & AuthInputFlags.StartJumping) != 0)
		    {
			    Player.Jump();
		    }
		    
		    LastVelocity = Player.Velocity;*/
	    }
	    
	    private void UpdateMouseInput(GameTime gt)
	    {
		    if (!CheckMovementInput)
			    return;
		    
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
					               *  Alex.DeltaTime;

					    look = -look;
						
					    Player.KnownPosition.HeadYaw = (Player.KnownPosition.HeadYaw - look.X) % 360f;
					    Player.KnownPosition.Pitch -= look.Y;
				    }
			    }

			    if (checkMouseInput)
			    {
				    var e = MouseInputListener.GetCursorPosition();
					
				    if (e.X < 10
				        || e.X > Graphics.Viewport.Width - 10
				        || e.Y < 10
				        || e.Y > Graphics.Viewport.Height - 10)
				    {
					    CenterCursor();
					    IgnoreNextUpdate = true;
				    }
				    else
				    {
					    var mouseDelta = e - _previousMousePosition;
					    //_previousMousePosition
					    //- e;

					    if (_invertX)
						    mouseDelta.X = -mouseDelta.X;

					    if (_invertY)
						    mouseDelta.Y = -mouseDelta.Y;

					    mouseDelta *= Alex.DeltaTime;

					    var look = (new Vector2((mouseDelta.X), (mouseDelta.Y)) * (float) CursorSensitivity);
					    Player.KnownPosition.HeadYaw = (Player.KnownPosition.HeadYaw - look.X) % 360f;
					    Player.KnownPosition.SetPitchBounded(Player.KnownPosition.Pitch - look.Y);
					    _previousMousePosition = e;
				    }
			    }
		    }
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

	    public void SetRewindHistorySize(int rewindHistorySize)
	    {
		    
	    }
    }
}
