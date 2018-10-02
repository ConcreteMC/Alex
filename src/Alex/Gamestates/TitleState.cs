using System;
using System.Drawing;
using System.IO;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Gamestates.Login;
using Alex.GameStates.Gui.Common;
using Alex.GameStates.Gui.Multiplayer;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Networking.Java;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Generators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.GameStates
{
	public class TitleState : GuiGameStateBase
	{
		private readonly GuiDebugInfo _debugInfo;

		private readonly GuiStackMenu _mainMenu;
		private readonly GuiStackMenu _debugMenu;
		private readonly GuiStackMenu _spMenu;

		private readonly GuiTextElement _splashText;

		private readonly GuiPanoramaSkyBox _backgroundSkyBox;
		private GuiEntityModelView _playerView;

		private readonly GuiImage _logo;
		private IPlayerProfileService _playerProfileService;

		private FpsMonitor FpsMonitor { get; }

		public TitleState()
		{
			FpsMonitor = new FpsMonitor();
			_backgroundSkyBox = new GuiPanoramaSkyBox(Alex);

			Background.Texture = _backgroundSkyBox;
			Background.RepeatMode = TextureRepeatMode.Stretch;

			#region Create MainMenu

			_mainMenu = new GuiStackMenu()
			{
				Margin = new Thickness(15, 0, 15, 0),
				Padding = new Thickness(0, 50, 0, 0),
				Width = 125,
				Anchor = Alignment.FillY | Alignment.MinX,

				ChildAnchor = Alignment.CenterY | Alignment.FillX,
				BackgroundOverlay = new Color(Color.Black, 0.35f)
			};

			_mainMenu.AddMenuItem("Multiplayer", OnMultiplayerButtonPressed, EnableMultiplayer);
			_mainMenu.AddMenuItem("SinglePlayer", OnSinglePlayerPressed);

			_mainMenu.AddMenuItem("Options", () => { Alex.GameStateManager.SetActiveState("options"); });
			_mainMenu.AddMenuItem("Exit", () => { Alex.Exit(); });

			#endregion

			#region Create DebugMenu

			_debugMenu = new GuiStackMenu()
			{
				Margin = new Thickness(15, 0, 15, 0),
				Padding = new Thickness(0, 50, 0, 0),
				Width = 125,
				Anchor = Alignment.FillY | Alignment.MinX,

				ChildAnchor = Alignment.CenterY | Alignment.FillX,
				BackgroundOverlay = new Color(Color.Black, 0.35f),

			};

			_debugMenu.AddMenuItem("Debug Blockstates", DebugWorldButtonActivated);
			_debugMenu.AddMenuItem("Debug Flatland", DebugFlatland);
			_debugMenu.AddMenuItem("Debug Anvil", DebugAnvil);
			_debugMenu.AddMenuItem("Go Back", DebugGoBackPressed);

			#endregion

			#region Create SPMenu

			_spMenu = new GuiStackMenu()
			{
				Margin = new Thickness(15, 0, 15, 0),
				Padding = new Thickness(0, 50, 0, 0),
				Width = 125,
				Anchor = Alignment.FillY | Alignment.MinX,

				ChildAnchor = Alignment.CenterY | Alignment.FillX,
				BackgroundOverlay = new Color(Color.Black, 0.35f),
			};

			_spMenu.AddMenuItem("SinglePlayer", () => {}, false);
			_spMenu.AddMenuItem("Debug Worlds", OnDebugPressed);

			_spMenu.AddMenuItem("Return to main menu", SpBackPressed);

			#endregion

			CreateProtocolMenu();

			AddChild(_mainMenu);

			AddChild(_logo = new GuiImage(GuiTextures.AlexLogo)
			{
				Margin = new Thickness(95, 25, 0, 0),
				Anchor = Alignment.TopCenter
			});

			AddChild(_splashText = new GuiTextElement()
			{
				TextColor = TextColor.Yellow,
				Rotation = 17.5f,

				Margin = new Thickness(240, 15, 0, 0),
				Anchor = Alignment.TopCenter,

				Text = "Who liek minecwaf?!"
			});


			_debugInfo = new GuiDebugInfo();
			_debugInfo.AddDebugRight(() =>
				$"Cursor RenderPosition: {Alex.InputManager.CursorInputListener.GetCursorPosition()} / {Alex.GuiManager.FocusManager.CursorPosition}");
			_debugInfo.AddDebugRight(() =>
				$"Cursor Delta: {Alex.InputManager.CursorInputListener.GetCursorPositionDelta()}");
			_debugInfo.AddDebugRight(() => $"Splash Text Scale: {_splashText.Scale:F3}");
			_debugInfo.AddDebugLeft(() => $"FPS: {FpsMonitor.Value:F0}");


			_playerProfileService = Alex.Services.GetService<IPlayerProfileService>();
			_playerProfileService.ProfileChanged += PlayerProfileServiceOnProfileChanged;
		}

		private bool _mpEnabled = true;
		public bool EnableMultiplayer
		{
			get { return _mpEnabled; }
			set
			{
				_mpEnabled = value;
				if (!value)
				{
					RemoveChild(_mainMenu);
					AddChild(_spMenu);
				}
				else
				{
					DebugGoBackPressed();
				}
			}
		}

		#region ProtocolMenu

		private GuiStackMenu _protocolMenu;
		private void CreateProtocolMenu()
		{
			_protocolMenu = new GuiStackMenu()
			{
				Margin = new Thickness(15, 0, 15, 0),
				Padding = new Thickness(0, 50, 0, 0),
				Width = 125,
				Anchor = Alignment.FillY | Alignment.MinX,

				ChildAnchor = Alignment.CenterY | Alignment.FillX,
				BackgroundOverlay = new Color(Color.Black, 0.35f),
			};

			_protocolMenu.AddMenuItem($"Java - Version {JavaProtocol.FriendlyName}", JavaEditionButtonPressed);
			_protocolMenu.AddMenuItem($"Bedrock - Unavailable", BedrockEditionButtonPressed, false);

			_protocolMenu.AddMenuItem("Return to main menu", ProtocolBackPressed);
		}

		private void ProtocolBackPressed()
		{
			RemoveChild(_protocolMenu);
			AddChild(_mainMenu);
		}

		private void SinglePlayerButtonPressed()
		{
			Alex.GameStateManager.SetActiveState<TitleState>();
			/*Alex.GameStateManager.SetAndUpdateActiveState<TitleState>(state =>
			{
				state.EnableMultiplayer = false;
				return state;
			});*/
		}

		private void BedrockEditionButtonPressed()
		{

		}

		private void JavaEditionButtonPressed()
		{
			Alex.GameStateManager.SetActiveState(new JavaLoginState(_backgroundSkyBox)
			{
				BackgroundOverlay = BackgroundOverlay
			}, true);
		}

		#endregion

		private void PlayerProfileServiceOnProfileChanged(object sender, PlayerProfileChangedEventArgs e)
		{
			_playerView.Entity = new PlayerMob(e.Profile.Username, null, null, e.Profile.Skin.Texture, e.Profile.Skin.Slim);
		}

		private void OnSinglePlayerPressed()
		{
			RemoveChild(_mainMenu);
			AddChild(_spMenu);
		}

		private void SpBackPressed()
		{
			RemoveChild(_spMenu);
			AddChild(_mainMenu);
		}

		private void DebugGoBackPressed()
		{
			RemoveChild(_debugMenu);
			AddChild(_spMenu);
		}

		private void OnDebugPressed()
		{
			RemoveChild(_spMenu);
			AddChild(_debugMenu);
		}

		private void OnMultiplayerButtonPressed()
		{
			RemoveChild(_mainMenu);
			AddChild(_protocolMenu);
		//	Alex.GameStateManager.SetActiveState("selectGameVersion");
		}

		protected override void OnLoad(IRenderArgs args)
		{
			Skin skin = Alex.ProfileManager.ActiveProfile?.Profile?.Skin;
			if (skin == null)
			{
				Alex.Resources.BedrockResourcePack.TryGetTexture("textures/entity/alex", out Bitmap rawTexture);
				skin = new Skin()
				{
					Slim = true,
					Texture = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, rawTexture)
				};
			}

			AddChild(_playerView = new GuiEntityModelView(new PlayerMob("", null, null, skin.Texture, skin.Slim)) /*"geometry.humanoid.customSlim"*/
			{
				BackgroundOverlay = new Color(Color.Black, 0.15f),

				Margin = new Thickness(15, 15, 5, 15),

				Width = 92,
				Height = 128,

				Anchor = Alignment.BottomRight,
			});

			using (MemoryStream ms = new MemoryStream(Resources.GradientBlur))
			{
				BackgroundOverlay = (TextureSlice2D)Texture2D.FromStream(args.GraphicsDevice, ms);
			}
			BackgroundOverlay.Mask = new Color(Color.White, 0.5f);

			_splashText.Text = SplashTexts.GetSplashText();
			Alex.IsMouseVisible = true;

			Alex.GameStateManager.AddState("serverlist", new MultiplayerServerSelectionState(_backgroundSkyBox));
		}

		private float _rotation;

		private readonly float _playerViewDepth = -512.0f;

		private KeyboardState _prevKeyboardState = new KeyboardState();
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			_backgroundSkyBox.Update(gameTime);

			_rotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds / (1000.0f / 20.0f);

			_splashText.Scale = 0.65f + (float)Math.Abs(Math.Sin(MathHelper.ToRadians(_rotation * 10.0f))) * 0.5f;

			var mousePos = Alex.InputManager.CursorInputListener.GetCursorPosition();

			mousePos = Vector2.Transform(mousePos, Alex.GuiManager.ScaledResolution.InverseTransformMatrix);
			var playerPos = _playerView.RenderBounds.Center.ToVector2();

			var mouseDelta = (new Vector3(playerPos.X, playerPos.Y, _playerViewDepth) - new Vector3(mousePos.X, mousePos.Y, 0.0f));
			mouseDelta.Normalize();

			var headYaw = (float)mouseDelta.GetYaw();
			var pitch = (float)mouseDelta.GetPitch();
			var yaw = (float)headYaw;

			_playerView.SetEntityRotation(-yaw, -pitch, -headYaw);

			KeyboardState s = Keyboard.GetState();
			if (_prevKeyboardState.IsKeyDown(Keys.M) && s.IsKeyUp(Keys.M))
			{
				_debugMenu.ModernStyle = !_debugMenu.ModernStyle;
				_mainMenu.ModernStyle = !_mainMenu.ModernStyle;
			}

			if (_prevKeyboardState.IsKeyDown(Keys.End) && s.IsKeyUp(Keys.End))
			{
				if (Alex.GuiManager.HasScreen(_debugInfo))
				{
					Alex.GuiManager.RemoveScreen(_debugInfo);
				}
				else
				{
					Alex.GuiManager.AddScreen(_debugInfo);
				}
			}

			_prevKeyboardState = s;
		}

		protected override void OnDraw(IRenderArgs args)
		{
			if (!_backgroundSkyBox.Loaded)
			{
				_backgroundSkyBox.Load(Alex.GuiRenderer);
			}

			_backgroundSkyBox.Draw(args);

			base.OnDraw(args);
			FpsMonitor.Update();
		}

		protected override void OnShow()
		{
			base.OnShow();
			Alex.GuiManager.AddScreen(_debugInfo);
		}

		protected override void OnHide()
		{
			Alex.GuiManager.RemoveScreen(_debugInfo);
			base.OnHide();
		}

		private void Debug(IWorldGenerator generator)
		{
			Alex.IsMultiplayer = false;

			Alex.IsMouseVisible = false;

			generator.Initialize();
			var debugProvider = new SPWorldProvider(Alex, generator);
			Alex.LoadWorld(debugProvider, debugProvider.Network);
		}

		private void DebugFlatland()
		{
			Debug(new FlatlandGenerator());
		}

		private void DebugAnvil()
		{
			Debug(new AnvilWorldProvider(Alex.GameSettings.Anvil)
			{
				MissingChunkProvider = new VoidWorldGenerator()
			});
		}

		private void DebugWorldButtonActivated()
		{
			Debug(new DebugWorldGenerator());
		}
	}
}
