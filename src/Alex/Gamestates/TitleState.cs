using System;
using System.Collections.Generic;
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
using Alex.Gui.Elements.Context3D;
using Alex.Items;
using Alex.Utils.Inventories;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Singleplayer;
using Alex.Worlds.Singleplayer.Generators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NLog;
using RocketUI;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Gamestates
{
	public class TitleState : GuiGameStateBase, IMenuHolder
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(TitleState));

		private readonly GuiStackMenu _mainMenu;

		private readonly GuiTextElement _splashText;

		private readonly GuiPanoramaSkyBox _backgroundSkyBox;
		private GuiEntityModelView _playerView;
		private IPlayerProfileService _playerProfileService;

		public TitleState()
		{
			_backgroundSkyBox = new GuiPanoramaSkyBox(Alex);

			Background.Texture = _backgroundSkyBox;
			Background.RepeatMode = TextureRepeatMode.Stretch;

			MenuItem baseMenu = new MenuItem(MenuType.Menu)
			{
				Children =
				{
					new MenuItem()
					{
						Title = "menu.multiplayer",
						OnClick = MultiplayerButtonPressed,
						IsTranslatable = true
					},
					new MenuItem(MenuType.SubMenu)
					{
						Title = "Debugging",
						IsTranslatable = false,
						Children =
						{
							new MenuItem()
							{
								Title = "Blockstates",
								OnClick = (sender, args) =>
								{
									Debug(new DebugWorldGenerator());
								}
							},
							new MenuItem()
							{
								Title = "Demo",
								OnClick = (sender, args) =>
								{
									Debug(new DemoGenerator());
								}
							},
							new MenuItem()
							{
								Title = "Flatland",
								OnClick = (sender, args) =>
								{
									Debug(new FlatlandGenerator());
								}
							},
							new MenuItem()
							{
								Title = "Chunk Debug",
								OnClick = (sender, args) =>
								{
									Debug(new ChunkDebugWorldGenerator());
								}
							}
						}
					},
					new MenuItem()
					{
						Title = "menu.options",
						OnClick = (sender, args) =>
						{
							Alex.GameStateManager.SetActiveState("options");
						},
						IsTranslatable = true
					},
					new MenuItem()
					{
						Title = "menu.quit",
						OnClick = (sender, args) =>
						{
							Alex.Exit();
						},
						IsTranslatable = true
					},
				}
			};

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

			ShowMenu(baseMenu);
			
			AddChild(_mainMenu);
			
			#endregion

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

			_playerProfileService = Alex.Services.GetService<IPlayerProfileService>();
			_playerProfileService.ProfileChanged += PlayerProfileServiceOnProfileChanged;
			
			/*ScoreboardView scoreboardView;
			AddChild(scoreboardView = new ScoreboardView());
			scoreboardView.Anchor = Alignment.MiddleRight;
			
			scoreboardView.AddString("Title");
			scoreboardView.AddRow("Key", "200");
			scoreboardView.AddRow("Key 2", "200");*/
		}

		private void MultiplayerButtonPressed(object sender, MenuItemClickedEventArgs e)
		{
			MultiplayerButtonPressed();
		}

		#region ProtocolMenu
		
		private void MultiplayerButtonPressed()
		{
			Alex.GameStateManager.SetActiveState(new MultiplayerServerSelectionState(_backgroundSkyBox)
			{
				BackgroundOverlay = BackgroundOverlay
			}, true);
		}

		#endregion

		private void PlayerProfileServiceOnProfileChanged(object sender, PlayerProfileChangedEventArgs e)
		{
			if (e.Profile?.Skin?.Texture != null)
			{
				_playerView.Entity = new RemotePlayer(e.Profile.Username, null, null, e.Profile.Skin.Texture,
					e.Profile.Skin.Slim ? "geometry.humanoid.customSlim" : "geometry.humanoid.custom" );
				_playerView.Entity.SetInventory(new BedrockInventory(46));
				
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

			var entity = new RemotePlayer("", null, null, skin.Texture);

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
				_mainMenu.ModernStyle = !_mainMenu.ModernStyle;
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
			
			_playerView.Entity.SetInventory(new BedrockInventory(46));
				
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

		private LinkedList<MenuItem> _menu = new LinkedList<MenuItem>();
		/// <inheritdoc />
		public void ShowMenu(MenuItem menu)
		{
			bool isFirst = (_menu.Count == 0 || _menu.First.Value == menu);
			
			if (!_menu.Contains(menu))
			{
				_menu.AddLast(menu);
			}

			_mainMenu.ClearChildren();

			foreach (var menuItem in menu.BuildMenu(this, BuildMode.Children))
			{
				_mainMenu.AddChild(menuItem);
			}

			if (!isFirst)
			{
				_mainMenu.AddMenuItem("Back", () =>
				{
					GoBack();
				});
			}
		}

		/// <inheritdoc />
		public bool GoBack()
		{
			if (_menu.Count > 1)
			{
				_menu.RemoveLast();
				ShowMenu(_menu.Last.Value);

				return true;
			}

			return false;
		}
	}
}
