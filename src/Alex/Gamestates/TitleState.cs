using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Timers;
using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Gui.Graphics;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Gamestates.Common;
using Alex.Gamestates.Login;
using Alex.Gamestates.MainMenu;
using Alex.Gamestates.Multiplayer;
using Alex.Graphics.Models.Entity;
using Alex.Gui;
using Alex.Gui.Dialogs;
using Alex.Gui.Dialogs.Containers;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Context3D;
using Alex.Gui.Elements.Hud;
using Alex.Items;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Utils.Inventories;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Singleplayer;
using Alex.Worlds.Singleplayer.Generators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MiNET.Utils;
using NLog;
using RocketUI;
using Color = Microsoft.Xna.Framework.Color;
using Skin = Alex.Common.Utils.Skin;
using UUID = MiNET.Utils.UUID;


namespace Alex.Gamestates
{
    public class TitleState : GuiGameStateBase, IMenuHolder
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(TitleState));

        private readonly StackMenu _mainMenu;

        private readonly TextElement _splashText;

        private readonly GuiPanoramaSkyBox     _backgroundSkyBox;
        private          GuiEntityModelView    _playerView;
        private          ProfileManager _profileManager;

        //private BossBarContainer _bossBarContainer;
        private System.Timers.Timer _switchItemTimer;
        public TitleState()
        {
            _switchItemTimer = new System.Timers.Timer();
            _switchItemTimer.Interval = 1000;
            _switchItemTimer.Elapsed += SwitchHeldItem;

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
						Visible = !VersionUtils.IsReleaseBuild,
                        Children =
                        {
                            new MenuItem()
                            {
                                Title = "Blockstates",
                                OnClick = (sender, args) => { Debug(new DebugWorldGenerator()); }
                            }
                        }
                    },
                    new MenuItem()
                    {
                        Title = "menu.options",
                        OnClick = (sender, args) => { Alex.GameStateManager.SetActiveState(new OptionsState(_backgroundSkyBox)); },
                        IsTranslatable = true
                    },
                    new MenuItem()
                    {
                        Title = "menu.quit",
                        OnClick = (sender, args) => { Alex.Exit(); },
                        IsTranslatable = true
                    },
                }
            };

            #region Create MainMenu

            _mainMenu = new StackMenu()
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

            AddChild(new Image(AlexGuiTextures.AlexLogo)
            {
                Margin = new Thickness(95, 25, 0, 0),
                Anchor = Alignment.TopCenter
            });

            AddChild(_splashText = new TextElement()
            {
                TextColor = (Color) TextColor.Yellow,
                Rotation = 17.5f,

                Margin = new Thickness(275, 15, 0, 0),
                Anchor = Alignment.TopCenter,

                Text = "Who liek minecwaf?!",
            });

            var guiItemStack = new StackContainer()
            {
                Anchor = Alignment.CenterX | Alignment.CenterY,
                Orientation = Orientation.Vertical
            };

            AddChild(guiItemStack);

            var row = new StackContainer()
            {
                Orientation = Orientation.Horizontal,
                Anchor = Alignment.TopFill,
                ChildAnchor = Alignment.FillCenter,
                Margin = Thickness.One
            };
            guiItemStack.AddChild(row);

            _profileManager = Alex.Services.GetRequiredService<ProfileManager>();
            //_playerProfileService.ProfileChanged += PlayerProfileServiceOnProfileChanged;

            var dropDown = new GuiDropdown() { };
            dropDown.Options.Add("option 1");
            dropDown.Options.Add("option 2");
            dropDown.Options.Add("option 3");
            dropDown.Value = 0;
            
            dropDown.Anchor = Alignment.MiddleCenter;
            
          //  AddChild(dropDown);
            /*ScoreboardView scoreboardView;
            AddChild(scoreboardView = new ScoreboardView());
            scoreboardView.Anchor = Alignment.MiddleRight;
            
            scoreboardView.AddString("Title");
            scoreboardView.AddRow("Key", "200");
            scoreboardView.AddRow("Key 2", "200");*/
        }

        private void SwitchHeldItem(object sender, ElapsedEventArgs e)
        {
            var inventory = _playerView?.Entity?.Inventory;

            if (inventory == null)
                return;
            
            inventory.SelectedSlot++;

            //inventory.SelectedSlot += 1;
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

        private void ApplyModel(Entity entity)
        {
            if (Alex.PlayerModel != null && Alex.PlayerTexture != null)
            {
                if (EntityModelRenderer.TryGetRenderer(Alex.PlayerModel, out var renderer))
                {
                    entity.Texture = TextureUtils.BitmapToTexture2D(this, Alex.GraphicsDevice, Alex.PlayerTexture);
                    entity.ModelRenderer = renderer;
                }
            }
        }

        protected override void OnLoad(IRenderArgs args)
        {
            var entity = new RemotePlayer(null);
            entity.RenderLocation = new PlayerLocation(Vector3.Zero, 180f, 180f);
            ApplyModel(entity);
            
            AddChild(_playerView =
                new GuiEntityModelView(
                    entity)
                {
                    BackgroundOverlay = new Color(Color.Black, 0.15f),

                    Margin = new Thickness(15, 15, 5, 40),

                    Width = 92,
                    Height = 128,

                    Anchor = Alignment.BottomRight,
                });

            AddChild(new AlexButton("Change Skin", ChangeSKinBtnPressed)
            {
                Anchor = Alignment.BottomRight,
                TranslationKey = "",
                Margin = new Thickness(15, 15, 6, 15),
                Width = 90,
                //Enabled = false
            }.ApplyModernStyle(false));

            BackgroundOverlay.TextureResource = AlexGuiTextures.GradientBlur;
            BackgroundOverlay.RepeatMode = TextureRepeatMode.Stretch;
            BackgroundOverlay.Mask = new Color(Color.White, 0.5f);

            _splashText.Text = SplashTexts.GetSplashText();
            Alex.IsMouseVisible = true;

            Alex.GameStateManager.AddState("serverlist", new MultiplayerServerSelectionState(_backgroundSkyBox));
        }

        private void ChangeSKinBtnPressed()
        {
            Alex.GameStateManager.SetActiveState(new SkinSelectionState(_backgroundSkyBox, Alex), true);
        }

        private float _rotation;

        private readonly float _playerViewDepth = -512.0f;

        private KeyboardState _prevKeyboardState = new KeyboardState();

        protected override void OnUpdate(GameTime gameTime)
        
        {
            base.OnUpdate(gameTime);

            _backgroundSkyBox.Update(gameTime);

            _rotation += (float) gameTime.ElapsedGameTime.TotalSeconds;

            _splashText.Scale = 0.65f + (float) Math.Abs(Math.Sin(MathHelper.ToRadians(_rotation * 60.0f))) * 0.5f;

            var mousePos = Alex.GuiManager.FocusManager.CursorPosition;

            mousePos = GuiRenderer.Unproject(mousePos);
            var playerPos = _playerView.RenderBounds.Center.ToVector2();

            var mouseDelta = (new Vector3(playerPos.X, playerPos.Y, _playerViewDepth) -
                              new Vector3(mousePos.X, mousePos.Y, 0.0f));
            mouseDelta.Normalize();

            var headYaw = (float) mouseDelta.GetYaw();
            var pitch   = (float) mouseDelta.GetPitch();
            var yaw     = (float) headYaw;

            _playerView.SetEntityRotation(yaw, pitch, headYaw);
            _playerView.Entity.RenderLocation.Yaw = yaw;
            _playerView.Entity.RenderLocation.HeadYaw = headYaw;
            _playerView.Entity.RenderLocation.Pitch = pitch;
            KeyboardState s = Keyboard.GetState();

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
            _playerView.Entity.SetInventory(new BedrockInventory(46));
            
            var inventory = _playerView.Entity.Inventory;
            
            int slot = inventory.HotbarOffset;
            if (ItemFactory.TryGetItem("minecraft:diamond_sword", out var sword))
                inventory.SetSlot(slot++, sword, false);

            if (ItemFactory.TryGetItem("minecraft:glass_pane", out var pane))
                inventory.SetSlot(slot++, pane, false);

            if (ItemFactory.TryGetItem("minecraft:oak_stairs", out var stairs))
                inventory.SetSlot(slot++, stairs, false);

            FastRandom fr = FastRandom.Instance;
            for (int i = slot; i < 8; i++)
            {
                Item item;

                do
                {
                    item = ItemFactory.AllItems[fr.Next() % ItemFactory.AllItems.Length];
                } while (item.IsAir() || item.Renderer == null);

                inventory.SetSlot(i, item, false);
            }

            ApplyModel(_playerView.Entity);
            _switchItemTimer.Start();
           // Alex.Instance.GuiManager.ShowDialog(new BrowserDialog("Microsoft Login", "https://google.com"));
            base.OnShow();
        }

        /// <inheritdoc />
        protected override void OnHide()
        {
            _switchItemTimer.Stop();
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
                _mainMenu.AddChild(new AlexButton("Back", () =>
                {
                    GoBack();
                }));
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