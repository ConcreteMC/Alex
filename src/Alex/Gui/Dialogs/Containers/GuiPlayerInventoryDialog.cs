using System;
using System.Linq;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Dialogs;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Graphics.Models.Entity;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Inventory;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;
using GuiCursorEventArgs = Alex.API.Gui.Events.GuiCursorEventArgs;

namespace Alex.Gui.Dialogs.Containers
{
    public class GuiPlayerInventoryDialog : GuiDialogBase
    {
        protected Player Player { get; }
        protected Inventory Inventory { get; }


        private InventoryContainerItem[] _guiHotBarInventoryItems;
        private GuiEntityModelView _playerEntityModelView;

        private GuiAutoUpdatingTextElement _debug;

        private const int ItemSize = 16;
        
        private GuiTextElement TextOverlay { get; }
        public GuiPlayerInventoryDialog(Player player, Inventory inventory)
        {
            Player = player;
            Inventory = inventory;

            // Subscribe to events

            _guiHotBarInventoryItems = new InventoryContainerItem[inventory?.SlotCount ?? 0];

            if(_guiHotBarInventoryItems.Length != 46) throw new ArgumentOutOfRangeException(nameof(inventory), inventory?.SlotCount ?? 0, "Expected player inventory containing 46 slots.");

            ContentContainer.Background = GuiTextures.InventoryPlayerBackground;
            ContentContainer.Width = ContentContainer.MinWidth = ContentContainer.MaxWidth = 176;
            ContentContainer.Height = ContentContainer.MinHeight = ContentContainer.MaxHeight = 166;
            
            SetFixedSize(176, 166);
            
            ContentContainer.AutoSizeMode = AutoSizeMode.None;

            AddChild(_debug = new GuiAutoUpdatingTextElement(() =>
            {
                if (base.GuiRenderer == null) return "";
                
                var position = Mouse.GetState().Position;
                
                return $"Cursor: {position}";
            }, true)
            {
                Background = new Color(Color.Black, 0.35f),
                Anchor = Alignment.TopCenter,
                Margin = new Thickness(0, 0, 0, 200)
            });

            var texture = player.ModelRenderer.Texture;
            if (texture == null)
            {
                
            }
            
            var modelRenderer = player.ModelRenderer;
            var mob = new PlayerMob(player.Name, player.Level, player.Network,
                player.ModelRenderer.Texture, true)
            {
                ModelRenderer = modelRenderer,
            };

            ContentContainer.AddChild(_playerEntityModelView =
                new GuiEntityModelView(mob)
                {
                    Margin = new Thickness(7, 25),
                    Width = 49,
                    Height = 70,
                    Anchor = Alignment.TopLeft,
                    AutoSizeMode = AutoSizeMode.None,
                    Background = null,
                    BackgroundOverlay =  null
                });

            int lx = 7, ly = 83;

            int idx = 9;
            Color color = Color.Blue;
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    var item = new InventoryContainerItem()
                    {
                        Item = Inventory[idx],
                        HighlightedBackground = new Microsoft.Xna.Framework.Color(color, 0.5f),
                        Anchor = Alignment.TopLeft,
                        AutoSizeMode = AutoSizeMode.None,
                      //  Name = idx.ToString(),
                        Margin = new Thickness(lx, ly, 0, 0)
                    };

                    _guiHotBarInventoryItems[idx] = item;
                    
                    ContentContainer.AddChild(item);
                    idx++;
                    
                    lx += item.Width;
                }

                lx = 7;
                
                if (idx == 36)
                {
                    idx = 0;
                    ly = 141;
                    color = Color.GreenYellow;
                }
                else
                {
                    ly += _guiHotBarInventoryItems[idx - 1].Height;
                }
            }

            lx = 7;
            ly = 7;
            for (int i = 0; i < 4; i++)
            {
                var element = new InventoryContainerItem()
                {
                    HighlightedBackground = new Microsoft.Xna.Framework.Color(Color.Red, 0.5f),
                    Anchor = Alignment.TopLeft,
                    Margin =  new Thickness(ly, lx),
                    AutoSizeMode = AutoSizeMode.None,
                    //Item = Inventory[]
                };
                
                ContentContainer.AddChild(element);
                
                ly += element.Height;
            }
            
            var shieldSlot = new InventoryContainerItem()
            {
                HighlightedBackground = new Microsoft.Xna.Framework.Color(Color.Orange, 0.5f),
                Anchor = Alignment.TopLeft,
                Margin =  new Thickness(61, 76),
                AutoSizeMode = AutoSizeMode.None
            };
                
            ContentContainer.AddChild(shieldSlot);

            AddChild(TextOverlay = new GuiTextElement(true)
            {
                HasShadow = true,
                Background = new Color(Color.Black, 0.35f),
                Enabled = false,
                FontStyle = FontStyle.DropShadow,
                TextColor = TextColor.Yellow,
                ClipToBounds = false,
                //BackgroundOverlay = new Color(Color.Black, 0.35f),
            });

            foreach (var child in ContentContainer.ChildElements.Where(x => x is InventoryContainerItem).Cast<InventoryContainerItem>())
            {
                child.CursorPressed += InventoryItemPressed;
                child.CursorEnter += ChildOnCursorEnter;
                child.CursorLeave += ChildOnCursorLeave;
            }
        }

        private string _overlayText = string.Empty;
        private void SetOverlayText(InventoryContainerItem item)
        {
            _overlayText = item.Item?.DisplayName ?? item.Item?.Name;
            _overlayStart = 0;
            _nextUpdate = TimeSpan.Zero;
        }

        private InventoryContainerItem ActiveItem { get; set; } = null;
        private InventoryContainerItem HoverItem { get; set; } = null;
        
        private void ChildOnCursorLeave(object sender, GuiCursorEventArgs e)
        {
            if (sender == HoverItem)
            {
                HoverItem = null;
            }
            
            TextOverlay.IsVisible = false;
        }

        private void ChildOnCursorEnter(object sender, GuiCursorEventArgs e)
        {
            if (sender is InventoryContainerItem item)
            {
                TextOverlay.IsVisible = true;
                SetOverlayText(item);
                
                HoverItem = item;
            }
            //AddChild(TextOverlay);
        }

        private void InventoryItemPressed(object sender, GuiCursorEventArgs e)
        {
            if (sender is InventoryContainerItem item)
            {
                InventoryItemClicked(item);   
            }
        }
        
        private void InventoryItemClicked(InventoryContainerItem containerItem)
        {
            if (containerItem == null) //Release
            {
                ActiveItem = null;
                RemoveChild(TextOverlay);
                return;
            }

            ActiveItem = containerItem;
            SetOverlayText(containerItem);
        }

        private int _overlayStart = 0;
        private TimeSpan _nextUpdate = TimeSpan.MinValue;
        private bool _reverseMarqueue = false;
        private const int _marqueueLength = 25;
        private void Marqueue(GameTime gt)
        {
            if (_nextUpdate < gt.TotalGameTime)
            {
                string text = _overlayText;
                if (!string.IsNullOrWhiteSpace(text) && text.Length > _marqueueLength)
                {
                    string overlayText = text.Substring(_overlayStart,
                        Math.Min(_marqueueLength, Math.Max(0, text.Length - _overlayStart)));

                    TextOverlay.Text = _reverseMarqueue ? $"...{overlayText}" : $"{overlayText}...";
                    
                    if (_reverseMarqueue)
                    {
                        _overlayStart--;
                        if (_overlayStart <= 0)
                        {
                            _overlayStart = 0;
                            _reverseMarqueue = false;
                            _nextUpdate = gt.TotalGameTime + TimeSpan.FromMilliseconds(1500);
                            return;
                        }
                    }
                    else
                    {
                        _overlayStart++;
                    }

                    if (text.Length - _overlayStart <= _marqueueLength)
                    {
                        _overlayStart--;
                        _reverseMarqueue = true;
                        _nextUpdate = gt.TotalGameTime + TimeSpan.FromMilliseconds(1500);
                        return;
                    }
                }
                else if (text != null && text.Length < _marqueueLength)
                {
                    TextOverlay.Text = text;
                }

                _nextUpdate = gt.TotalGameTime + TimeSpan.FromMilliseconds(500);
            }
        }
        
        private readonly float _playerViewDepth = -512.0f;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            var renderPos = HoverItem?.RenderPosition ?? ActiveItem?.RenderPosition;
            if (renderPos.HasValue)
            {
                TextOverlay.RenderPosition = renderPos.Value;
                
                Marqueue(gameTime);
            }
            
            var mousePos = Alex.Instance.InputManager.CursorInputListener.GetCursorPosition();

            mousePos = Vector2.Transform(mousePos, Alex.Instance.GuiManager.ScaledResolution.InverseTransformMatrix);
            var playerPos = _playerEntityModelView.RenderBounds.Center.ToVector2();

            var mouseDelta = (new Vector3(playerPos.X, playerPos.Y, _playerViewDepth) - new Vector3(mousePos.X, mousePos.Y, 0.0f));
            mouseDelta.Normalize();

            var headYaw = (float)mouseDelta.GetYaw();
            var pitch = (float)mouseDelta.GetPitch();
            var yaw = (float)headYaw;

            _playerEntityModelView.SetEntityRotation(-yaw, pitch, -headYaw);

            if (Inventory != null)
            {
                _playerEntityModelView.Entity.ShowItemInHand = true;
                
                _playerEntityModelView.Entity.Inventory[Inventory.SelectedSlot] = Inventory[Inventory.SelectedSlot];
                _playerEntityModelView.Entity.Inventory.MainHand = Inventory.MainHand;
                _playerEntityModelView.Entity.Inventory.SelectedSlot = Inventory.SelectedSlot;
            }
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
        }
    }
}
