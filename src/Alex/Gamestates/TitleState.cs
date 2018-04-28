using System;
using System.Drawing;
using System.IO;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.GameStates.Gui.Common;
using Alex.GameStates.Gui.Multiplayer;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Generators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.GameStates
{
	public class TitleState : GuiGameStateBase
	{
		private readonly GuiDebugInfo _debugInfo;

		private readonly GuiStackMenu _stackMenu;
		private readonly GuiTextElement _splashText;

		private readonly GuiPanoramaSkyBox _backgroundSkyBox;
		private readonly GuiEntityModelView _playerView;

		private readonly GuiImage _logo;

		private FpsMonitor FpsMonitor { get; }
		public TitleState()
		{
			FpsMonitor = new FpsMonitor();
			_backgroundSkyBox = new GuiPanoramaSkyBox(Alex);

			Background.Texture = _backgroundSkyBox;
			Background.RepeatMode = TextureRepeatMode.Stretch;


			AddChild(_stackMenu = new GuiStackMenu()
			{
				Margin = new Thickness(15, 0, 15, 0),
				Padding = new Thickness(0, 50, 0, 0),
				Width = 125,
				Anchor = Alignment.FillY | Alignment.MinX,

				ChildAnchor = Alignment.CenterY | Alignment.FillX,
				BackgroundOverlay = new Color(Color.Black, 0.35f)
			});

			_stackMenu.AddMenuItem("Multiplayer", OnMultiplayerButtonPressed);

			_stackMenu.AddMenuItem("Debug Blockstates", DebugWorldButtonActivated);
			_stackMenu.AddMenuItem("Debug Flatland", DebugFlatland);
			_stackMenu.AddMenuItem("Debug Anvil", DebugAnvil);

			_stackMenu.AddMenuItem("Options", () => { Alex.GameStateManager.SetActiveState("options"); });
			_stackMenu.AddMenuItem("Exit Game", () => { Alex.Exit(); });

			AddChild(_playerView = new GuiEntityModelView("geometry.humanoid.customSlim")
			{
				BackgroundOverlay = new Color(Color.Black, 0.15f),

				Margin = new Thickness(15),

				Width = 92,
				Height = 128,

				Anchor = Alignment.BottomRight,
			});

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

			AddChild(new GuiTextElement()
			{
				Anchor = Alignment.BottomRight,
				Text = "github.com/kennyvv/Alex",
				TextColor = TextColor.White,
				TextOpacity = 0.5f,
				Scale = 0.5f,
				Margin = new Thickness(0, 0, 30, 5)
			});

			_debugInfo = new GuiDebugInfo();
			_debugInfo.AddDebugRight(() => $"Cursor RenderPosition: {Alex.InputManager.CursorInputListener.GetCursorPosition()} / {Alex.GuiManager.FocusManager.CursorPosition}");
			_debugInfo.AddDebugRight(() => $"Cursor Delta: {Alex.InputManager.CursorInputListener.GetCursorPositionDelta()}");
			_debugInfo.AddDebugRight(() => $"Splash Text Scale: {_splashText.Scale:F3}");
			_debugInfo.AddDebugLeft(() => $"FPS: {FpsMonitor.Value:F0}");
		}

		private void OnMultiplayerButtonPressed()
		{
			Alex.GameStateManager.SetActiveState("serverlist");
		}

		protected override void OnLoad(IRenderArgs args)
		{
			Alex.Resources.BedrockResourcePack.TryGetTexture("textures/entity/alex", out Bitmap rawTexture);
			var steve = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, rawTexture);

			_playerView.SkinTexture = steve;

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

			_playerView.SetEntityRotation(yaw, pitch, headYaw);

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
