using System;
using System.IO;
using System.Threading;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Gamestates.Common;
using Alex.Gamestates.MainMenu;
using Alex.Gamestates.Multiplayer;
using Alex.Graphics.Models.Entity;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Items;
using Alex.Networking.Java;
using Alex.Services;
using Alex.Worlds;
using Alex.Worlds.Generators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MiNET.Net;
using NLog;
using RocketUI;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Gamestates
{
	public class TitleState : GuiGameStateBase
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(TitleState));

		private readonly GuiStackMenu _mainMenu;
		private readonly GuiStackMenu _debugMenu;
		private readonly GuiStackMenu _spMenu;
		
		private readonly GuiTextElement _splashText;

		private readonly GuiPanoramaSkyBox _backgroundSkyBox;
		private GuiEntityModelView _playerView;
		private IPlayerProfileService _playerProfileService;
		//private GuiItem _guiItem;
		//private GuiItem _guiItem2;
		public TitleState()
		{
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

			_mainMenu.AddMenuItem("menu.multiplayer", JavaEditionButtonPressed, EnableMultiplayer, true);
			_mainMenu.AddMenuItem("menu.singleplayer", OnSinglePlayerPressed, true, true);

			_mainMenu.AddMenuItem("menu.options", () => { Alex.GameStateManager.SetActiveState("options"); }, true, true);
			_mainMenu.AddMenuItem("menu.quit", () => { Alex.Exit(); }, true, true);
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
			_debugMenu.AddMenuItem("Demo", DemoButtonActivated);
			_debugMenu.AddMenuItem("Debug Flatland", DebugFlatland);
			//_debugMenu.AddMenuItem("Debug Anvil", DebugAnvil);
			_debugMenu.AddMenuItem("Debug Chunk", DebugChunkButtonActivated);
		//	_debugMenu.AddMenuItem("Debug XBL Login", BedrockEditionButtonPressed);
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

			AddChild(new GuiImage(GuiTextures.AlexLogo)
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

				Text = "Who liek minecwaf?!",
			});
			
			var guiItemStack = new GuiStackContainer()
			{
				Anchor = Alignment.CenterX | Alignment.CenterY,
				Orientation = Orientation.Vertical
			};
			
			AddChild(guiItemStack);
			
			var row = new GuiStackContainer() {
				Orientation = Orientation.Horizontal,
				Anchor = Alignment.TopFill,
				ChildAnchor = Alignment.FillCenter,
				Margin = Thickness.One
			};
			guiItemStack.AddChild(row);
			
			/*row.AddChild(_guiItem = new GuiItem()
			{
				Height = 24,
				Width = 24,
				Background = new Color(Color.Black, 0.2f)
			});
			row.AddChild(_guiItem2 = new GuiItem()
			{
				Height = 24,
				Width = 24,
				Background = new Color(Color.Black, 0.2f)
			});
			*/
		/*	guiItemStack.AddChild(new GuiVector3Control(() => _guiItem.Camera.Position, newValue =>
			{
				if (_guiItem.Camera != null)
				{
					_guiItem.Camera.Position = newValue;
				}
				if(_guiItem2.Camera != null)
				{
					_guiItem2.Camera.Position = newValue;
				}
			}, 0.25f)
			{
				Margin = new Thickness(2)
			});*/
			
			// guiItemStack.AddChild(new GuiVector3Control(() => _guiItem.Camera.TargetPositionOffset, newValue =>
			// {
			// 	if (_guiItem.Camera != null)
			// 	{
			// 		_guiItem.Camera.Target = newValue;
			// 	}
			// 	if(_guiItem2.Camera != null)
			// 	{
			// 		_guiItem2.Camera.Target = newValue;
			// 	}
			// }, 0.25f)
			// {
			// 	Margin = new Thickness(2)
			// });
			
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

			_protocolMenu.AddMenuItem($"Java - {JavaProtocol.FriendlyName}", JavaEditionButtonPressed);
			_protocolMenu.AddMenuItem($"Bedrock - {McpeProtocolInfo.GameVersion}", BedrockEditionButtonPressed, false);

			_protocolMenu.AddMenuItem("Return to main menu", ProtocolBackPressed);
		}

		private void ProtocolBackPressed()
		{
			RemoveChild(_protocolMenu);
			AddChild(_mainMenu);
		}

		private void BedrockEditionButtonPressed()
		{
			var client = Alex.Services.GetRequiredService<XBLMSAService>();
			var t = client.AsyncBrowserLogin();
		}

		private void JavaEditionButtonPressed()
		{
			Alex.GameStateManager.SetActiveState(new MultiplayerServerSelectionState(_backgroundSkyBox)
			{
				BackgroundOverlay = BackgroundOverlay
			}, true);
		}

		#endregion

		private void PlayerProfileServiceOnProfileChanged(object sender, PlayerProfileChangedEventArgs e)
		{
			if (e.Profile.Skin.Texture != null)
			{
				_playerView.Entity = new PlayerMob(e.Profile.Username, null, null, e.Profile.Skin.Texture,
					e.Profile.Skin.Slim ? "geometry.humanoid.customSlim" : "geometry.humanoid.custom" );
				
				_playerView.Entity.Inventory.IsPeInventory = true;
				_playerView.Entity.ShowItemInHand = true;

				if (ItemFactory.TryGetItem("minecraft:grass_block", out var grass))
				{
					_playerView.Entity.Inventory.MainHand = grass;
					_playerView.Entity.Inventory[_playerView.Entity.Inventory.SelectedSlot] = grass;
				}
				
				if (ItemFactory.TryGetItem("minecraft:diamond_sword", out var sword))
				{
					//_playerView.Entity.Inventory.MainHand = sword;
					//_playerView.Entity.Inventory[_playerView.Entity.Inventory.SelectedSlot] = sword;
				}
			}
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

		protected override void OnLoad(IRenderArgs args)
		{
			Skin skin = _playerProfileService?.CurrentProfile?.Skin;
			if (skin == null)
			{
				Alex.Resources.ResourcePack.TryGetBitmap("entity/alex", out var rawTexture);
				skin = new Skin()
				{
					Slim = true,
					Texture = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, rawTexture)
				};
			}

			var entity = new PlayerMob("", null, null, skin.Texture);
			entity.Inventory.IsPeInventory = true;
			entity.ShowItemInHand = true;

			AddChild(_playerView =
				new GuiEntityModelView(
						entity /*new PlayerMob("", null, null, skin.Texture, skin.Slim)*/) /*"geometry.humanoid.customSlim"*/
					{
						BackgroundOverlay = new Color(Color.Black, 0.15f),

						Margin = new Thickness(15, 15, 5, 40),

						Width = 92,
						Height = 128,

						Anchor = Alignment.BottomRight,
					});

			AddChild(new GuiButton("Change Skin", ChangeSKinBtnPressed)
			{
				Anchor = Alignment.BottomRight,
				Modern = false,
				TranslationKey = "",
				Margin = new Thickness(15, 15, 6, 15),
				Width = 90,
				//Enabled = false
			});

			AutoResetEvent reset = new AutoResetEvent(false);
			Alex.UIThreadQueue.Enqueue(() =>
			{
				using (MemoryStream ms =
					new MemoryStream(ResourceManager.ReadResource("Alex.Resources.GradientBlur.png")))
				{
					BackgroundOverlay = (TextureSlice2D) GpuResourceManager.GetTexture2D(this, args.GraphicsDevice, ms);
				}

				BackgroundOverlay.RepeatMode = TextureRepeatMode.Stretch;
				reset.Set();
			});
			reset.WaitOne();
			reset.Dispose();

			BackgroundOverlay.Mask = new Color(Color.White, 0.5f);

			_splashText.Text = SplashTexts.GetSplashText();
			Alex.IsMouseVisible = true;

			Alex.GameStateManager.AddState("serverlist", new MultiplayerServerSelectionState(_backgroundSkyBox));
			//Alex.GameStateManager.AddState("profileSelection", new ProfileSelectionState(_backgroundSkyBox));
		}

		private void ChangeSKinBtnPressed()
		{
			Alex.GameStateManager.SetActiveState(new SkinSelectionState(_backgroundSkyBox, Alex), true);
			//Alex.GameStateManager.SetActiveState(new ProfileSelectionState(_backgroundSkyBox, Alex), true);
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

			_playerView.SetEntityRotation(-yaw, pitch, -headYaw);

			KeyboardState s = Keyboard.GetState();
			if (_prevKeyboardState.IsKeyDown(Keys.M) && s.IsKeyUp(Keys.M))
			{
				_debugMenu.ModernStyle = !_debugMenu.ModernStyle;
				_mainMenu.ModernStyle = !_mainMenu.ModernStyle;
			}

			_prevKeyboardState = s;

		/*	if (_guiItem.Item == null)
			{
				if (ItemFactory.TryGetItem("minecraft:grass_block", out var item))
					_guiItem.Item = item;
				else
					_guiItem.Item = null;
			}
			if (_guiItem2.Item == null)
			{
				if (ItemFactory.TryGetItem("minecraft:diamond_hoe", out var item))
					_guiItem2.Item = item;
				else
					_guiItem2.Item = null;
			}*/
		}

		protected override void OnDraw(IRenderArgs args)
		{
			if (!_backgroundSkyBox.Loaded)
			{
				_backgroundSkyBox.Load(Alex.GuiRenderer);
			}

			_backgroundSkyBox.Draw(args);

			base.OnDraw(args);
		}

		protected override void OnShow()
		{
			if (Alex.PlayerModel != null && Alex.PlayerTexture != null)
			{
				Alex.UIThreadQueue.Enqueue(
					() =>
					{
						var texture = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, Alex.PlayerTexture);
						_playerView.Entity.ModelRenderer = new EntityModelRenderer(Alex.PlayerModel, texture);
					});
			}
			
			if (Alex.GameStateManager.TryGetState<OptionsState>("options", out _))
			{
				Alex.GameStateManager.RemoveState("options");
			}
			
			Alex.GameStateManager.AddState("options", new OptionsState(_backgroundSkyBox));
			
			base.OnShow();
		}

		protected override void OnHide()
		{
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
		
		private void DebugWorldButtonActivated()
		{
			Debug(new DebugWorldGenerator());
		}

		private void DebugChunkButtonActivated()
		{
			Debug(new ChunkDebugWorldGenerator());
		}

		private void DemoButtonActivated()
		{
			Debug(new DemoGenerator());
		}
	}
}
