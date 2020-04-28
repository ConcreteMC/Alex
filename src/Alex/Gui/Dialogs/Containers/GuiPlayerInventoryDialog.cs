using System;
using System.Linq;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Dialogs;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Graphics.Models.Entity;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Inventory;
using Alex.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NLog;
using RocketUI;
using GuiCursorEventArgs = Alex.API.Gui.Events.GuiCursorEventArgs;

namespace Alex.Gui.Dialogs.Containers
{
    public class GuiPlayerInventoryDialog : GuiDialogBase
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        protected Player Player { get; }
        protected Inventory Inventory { get; }


        private InventoryContainerItem[] _guiHotBarInventoryItems;
        private GuiEntityModelView _playerEntityModelView;

        private GuiAutoUpdatingTextElement _debug;

        private const int ItemSize = 16;
        
        private GuiTextElement TextOverlay { get; }
        private InventoryContainerItem CraftingOutput { get; }
        public GuiPlayerInventoryDialog(Player player, Inventory inventory)
        {
            Player = player;
            Inventory = inventory;

            // Subscribe to events

            _guiHotBarInventoryItems = new InventoryContainerItem[inventory?.SlotCount ?? 0];

            if(_guiHotBarInventoryItems.Length != 46) throw new ArgumentOutOfRangeException(nameof(inventory), inventory?.SlotCount ?? 0, "Expected player inventory containing 46 slots.");

            ContentContainer.Background = GuiTextures.InventoryPlayerBackground;
            ContentContainer.BackgroundOverlay = null;
            
            ContentContainer.Width = ContentContainer.MinWidth = ContentContainer.MaxWidth = 176;
            ContentContainer.Height = ContentContainer.MinHeight = ContentContainer.MaxHeight = 166;
            
            SetFixedSize(176, 166);
            
            ContentContainer.AutoSizeMode = AutoSizeMode.None;

           /* AddChild(_debug = new GuiAutoUpdatingTextElement(() =>
            {
                if (base.GuiRenderer == null) return "";
                
                var position = Mouse.GetState().Position;
                
                return $"Cursor: {position}";
            }, true)
            {
                Background = new Color(Color.Black, 0.35f),
                Anchor = Alignment.TopCenter,
                Margin = new Thickness(0, 0, 0, 200)
            });*/

            var texture = player.ModelRenderer.Texture;
            if (texture == null)
            {
                
            }
            
            var modelRenderer = player.ModelRenderer;
            var mob = new PlayerMob(player.Name, player.Level, player.Network,
                player.ModelRenderer.Texture)
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
                        Margin = new Thickness(lx, ly, 0, 0),
                        InventoryIndex = idx
                    };

                    _guiHotBarInventoryItems[idx] = item;
                    
                    ContentContainer.AddChild(item);
                    idx++;
                    
                    lx += item.Width;
                }

                lx = 7;
                
                if (idx == 36)
                {
                    if (Inventory.IsPeInventory)
                    {
                        idx = 0;
                    }

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
                    //Item = Inventory[39 - i],
                   // InventoryIndex = 39 - i
                };

                switch (i)
                {
                    case 0:
                        element.Item = Inventory.Helmet;
                        element.InventoryIndex = Inventory.HelmetSlot;
                        break;
                    case 1:
                        element.Item = Inventory.Chestplate;
                        element.InventoryIndex = Inventory.ChestSlot;
                        break;
                    case 2:
                        element.Item = Inventory.Leggings;
                        element.InventoryIndex = Inventory.LeggingsSlot;
                        break;
                    case 3:
                        element.Item = Inventory.Boots;
                        element.InventoryIndex = Inventory.BootsSlot;
                        break;
                }
                
                ContentContainer.AddChild(element);
                
                ly += element.Height;
            }
            
            lx = 97;
            ly = 17;

            int slotId = 41;
            for (int y = 0; y < 2; y++)
            {
                var height = 0;
                for (int x = 0; x < 2; x++)
                {
                    var element = new InventoryContainerItem()
                    {
                        HighlightedBackground = new Microsoft.Xna.Framework.Color(Color.Purple, 0.5f),
                        Anchor = Alignment.TopLeft,
                        Margin = new Thickness(ly, lx),
                        AutoSizeMode = AutoSizeMode.None,
                        Item = Inventory[slotId],
                        InventoryIndex = slotId
                    };

                    ContentContainer.AddChild(element);
                    lx += element.Width;
                    height = element.Height;

                    slotId++;
                }

                lx = 97;
                ly += height;
            }

            CraftingOutput = new InventoryContainerItem()
            {
                HighlightedBackground = new Microsoft.Xna.Framework.Color(Color.Purple, 0.5f),
                Anchor = Alignment.TopLeft,
                Margin = new Thickness(27, 153),
                AutoSizeMode = AutoSizeMode.None,
                Item = Inventory[45],
                InventoryIndex = 45
            };
            ContentContainer.AddChild(CraftingOutput);

            var shieldSlot = new InventoryContainerItem()
            {
                HighlightedBackground = new Microsoft.Xna.Framework.Color(Color.Orange, 0.5f),
                Anchor = Alignment.TopLeft,
                Margin =  new Thickness(61, 76),
                AutoSizeMode = AutoSizeMode.None,
                Item = Inventory[40],
                InventoryIndex = 40
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

        private InventoryContainerItem ActiveItem = null;
        private InventoryContainerItem HoverItem = null;
        private int _hoverSlot = 0;
        private void ChildOnCursorLeave(object sender, GuiCursorEventArgs e)
        {
            if (sender == HoverItem)
            {
                HoverItem = null;
                TextOverlay.IsVisible = ActiveItem != null;
            }
        }

        private void ChildOnCursorEnter(object sender, GuiCursorEventArgs e)
        {
            if (sender is InventoryContainerItem item)
            {
                _hoverSlot = item.InventoryIndex;
                HoverItem = item;
                
                if (item.Item.Count > 0)
                {
                    if (ActiveItem == null)
                    {
                        TextOverlay.IsVisible = true;
                        SetOverlayText(item);
                    }
                }
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
            var originalContainerItem = containerItem.Item;
            if (ActiveItem != null)
            {
                var oldItem = containerItem.Item;
                containerItem.Item = ActiveItem.Item;
                ActiveItem.Item = oldItem;

                Inventory[ActiveItem.InventoryIndex] = oldItem;
                Inventory[containerItem.InventoryIndex] = containerItem.Item;
                
                ActiveItem = null;
                /*if (HoverItem != null)
                {
                    var oldHoverItem = HoverItem.Item;
                    HoverItem.Item = ActiveItem.Item;
                }*/
            }

            if (originalContainerItem.Id > 0)
            {
                ActiveItem = containerItem;
                SetOverlayText(containerItem);
            }
            else
            {
                TextOverlay.IsVisible = false;
            }
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

        private void HandleSlotReplace()
        {
            var activeItem = ActiveItem;
            
            var originalHoverItem = HoverItem.Item;
            HoverItem.Item = activeItem.Item;
            Inventory[HoverItem.InventoryIndex] = activeItem.Item;
            Inventory[activeItem.InventoryIndex] = originalHoverItem;
            ActiveItem.Item = originalHoverItem;

            if (originalHoverItem.Id > 0 && originalHoverItem.Count > 0)
            {
                SetOverlayText(ActiveItem);
            }
            else
            {
                ActiveItem = null;
            }
        }
        
        private Item _cursorItem;
        private void MouseDown()
        {
            var hoverItem = HoverItem;
            Log.Info($"Mouse Down!");

            //We clicked while holding an item on the cursor
            if (ActiveItem != null)
            {
                var activeItem = ActiveItem;

                if (hoverItem != null)
                {
                    HandleSlotReplace();
                }
            }
            else //WE do not yet have a cursor item
            {
                if (hoverItem != null)
                {
                    if (hoverItem.Item.Id > 0 && hoverItem.Item.Count > 0)
                    {
                        ActiveItem = hoverItem;
                        // hoverItem.Item = new ItemAir();
                    
                        HoverItem = null;
                        SetOverlayText(hoverItem);
                        //InventoryItemClicked(HoverItem, )
                    }
                }
            }
        }

        private void MouseUp()
        {
            Log.Info($"Mouse released!");    
            if (ActiveItem != null)
            {
                var activeItem = ActiveItem;
                
                if (HoverItem != null)
                {
                    HandleSlotReplace();

                    // ActiveItem.Item = originalHoverItem.Item;
                    //  Inventory[ActiveItem.InventoryIndex] = originalHoverItem.Item;
                    //activeItem.
                    //if (originalHoverItem.)
                }
                else //We dropped the item outside of the inventory
                {
                    var originalItem = ActiveItem.Item;
                    Log.Info($"Dropped item: {originalItem}");
                    
                    ActiveItem.Item = new ItemAir();
                    Inventory[ActiveItem.InventoryIndex] = new ItemAir();

                    ActiveItem = null;
                    TextOverlay.IsVisible = false;
                }
            }
        }
        
        private readonly float _playerViewDepth = -512.0f;
        private bool _mouseDown = false;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            /*
            var renderPos = HoverItem?.RenderPosition ?? ActiveItem?.RenderPosition;
            if (renderPos.HasValue)
            {
                TextOverlay.RenderPosition = renderPos.Value;
                
                Marqueue(gameTime);
            }*/

            var cursorListener = Alex.Instance.InputManager.CursorInputListener;
            /*if (_mouseDown && cursorListener.IsUp(InputCommand.LeftClick))
            {
                _mouseDown = false;
                MouseUp();
                //MOUSE RELEASED.
            }
            else if (!_mouseDown && cursorListener.IsDown(InputCommand.LeftClick))
            {
                _mouseDown = true;
                MouseDown();
                //MOUSE CLICKED.
            }*/
            
            var mousePos = cursorListener.GetCursorPosition();

            mousePos = Vector2.Transform(mousePos, Alex.Instance.GuiManager.ScaledResolution.InverseTransformMatrix);

            TextOverlay.RenderPosition = mousePos;
            Marqueue(gameTime);
            
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
