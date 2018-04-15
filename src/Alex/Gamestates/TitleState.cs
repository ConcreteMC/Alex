using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Gamestates.Gui;
using Alex.GameStates.Gui.MainMenu;
using Alex.Graphics;
using Alex.Graphics.Models;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Worlds;
using Alex.Worlds.Generators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Gamestates
{
	public class TitleState : GameState
	{
		private GuiDebugInfo _debugInfo;

		private GuiTextElement _splashText;

		private GuiPanoramaSkyBox _backgroundSkyBox;
		private GuiEntityModelView _playerView;

		public TitleState(Alex alex, ContentManager content) : base(alex)
		{
			_backgroundSkyBox = new GuiPanoramaSkyBox(alex, alex.GraphicsDevice, content);

			Gui = new GuiScreen(Alex)
			{
				//DefaultBackgroundTexture = GuiTextures.OptionsBackground
			};
			var stackMenu = new GuiStackMenu()
			{
				LayoutOffsetX = 25,
				Width = 125,
				VerticalAlignment = VerticalAlignment.Center,

				VerticalContentAlignment = VerticalAlignment.Top,
				HorizontalContentAlignment = HorizontalAlignment.Stretch
			};

			stackMenu.AddMenuItem("Multiplayer", () =>
			{
				//TODO: Switch to multiplayer serverlist (maybe choose PE or Java?)
				Alex.ConnectToServer();
			});

			stackMenu.AddMenuItem("Multiplayer Servers", () =>
			{
				Alex.GameStateManager.SetActiveState<MultiplayerServerSelectionState>();
			});

			stackMenu.AddMenuItem("Debug Blockstates", DebugWorldButtonActivated);
			stackMenu.AddMenuItem("Debug Flatland", DebugFlatland);
			stackMenu.AddMenuItem("Debug Anvil", DebugAnvil);

			stackMenu.AddMenuItem("Options", () => { Alex.GameStateManager.SetActiveState("options"); });
			stackMenu.AddMenuItem("Exit Game", () => { Alex.Exit(); });

			Gui.AddChild(stackMenu);

			Gui.AddChild(new GuiImage(GuiTextures.AlexLogo)
			{
				//LayoutOffsetX = 175,
				LayoutOffsetY = 25,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Top
			});
			Gui.AddChild( _splashText = new GuiTextElement(false)
			{
				TextColor = TextColor.Yellow,
				Rotation = 17.5f,
				//RotationOrigin = Vector2.Zero,
				
				X = 240,
				Y = 15,

				Text = "Who liek minecwaf?!"
			});

			var username = alex.GameSettings.Username;
			Gui.AddChild(_playerView = new GuiEntityModelView("geometry.humanoid.custom")
			{
				BackgroundOverlayColor = new Microsoft.Xna.Framework.Color(Color.Black, 0.15f),

				LayoutOffsetX = -25,
				LayoutOffsetY = -25,

				Width = 64,
				Height = 128,

				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Bottom
			});

			_debugInfo = new GuiDebugInfo(alex);
			_debugInfo.AddDebugRight(() => $"Cursor Position: {alex.InputManager.CursorInputListener.GetCursorPosition()} / {alex.GuiManager.FocusManager.CursorPosition}");
			_debugInfo.AddDebugRight(() => $"Cursor Delta: {alex.InputManager.CursorInputListener.GetCursorPositionDelta()}");
			_debugInfo.AddDebugRight(() => $"Splash Text Scale: {_splashText.Scale:F3}");

		}
		
		public void LoadPlayerSkinCallback(Texture2D skinTexture)
		{
			//_playerView.LoadTexture(skinTexture);
		}

		protected override void OnLoad(RenderArgs args)
		{
			Alex.Resources.BedrockResourcePack.TryGetTexture("textures/entity/steve", out Bitmap rawTexture);
			var steve = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, rawTexture);

			_playerView.SkinTexture = steve;

			//var logo = new UiElement()
			//{
			//	ClassName = "TitleScreenLogo",
			//};
			//Gui.AddChild(logo);

			//SynchronizationContext.Current.Send((o) => _backgroundSkyBox.Load(Alex.GuiRenderer), null);

			Alex.IsMouseVisible = true;
		}

		private float _rotation;

		private float _playerViewDepth = -5.0f;

		protected override void OnUpdate(GameTime gameTime)
		{
			_backgroundSkyBox.Update(gameTime);

			_rotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds / (1000.0f / 20.0f);

			_splashText.Scale = 0.65f + (float)Math.Abs(Math.Sin(MathHelper.ToRadians(_rotation * 10.0f))) * 0.5f;

			var mousePos = Alex.InputManager.CursorInputListener.GetCursorPosition();
			var playerPos = _playerView.Bounds.Center.ToVector2();

			var mouseDelta = (new Vector3(playerPos.X, playerPos.Y, _playerViewDepth) - new Vector3(mousePos.X, mousePos.Y, 0.0f));
			mouseDelta.Normalize();

			_playerView.SetEntityRotation((float) mouseDelta.GetYaw(), (float) mouseDelta.GetPitch());
			
			base.OnUpdate(gameTime);
		}

		protected override void OnDraw3D(RenderArgs args)
		{
			if (!_backgroundSkyBox.Loaded)
			{
				_backgroundSkyBox.Load(Alex.GuiRenderer);
			}

			_backgroundSkyBox.Draw(args);

			base.OnDraw3D(args);
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
