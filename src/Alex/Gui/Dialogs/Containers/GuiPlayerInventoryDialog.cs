using System;
using Alex.API.Gui.Dialogs;
using Alex.API.Gui.Graphics;
using Alex.Entities;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Inventory;
using Alex.Utils;
using RocketUI;

namespace Alex.Gui.Dialogs.Containers
{
    public class GuiPlayerInventoryDialog : GuiDialogBase
    {
        protected Player Player { get; }
        protected Inventory Inventory { get; }


        private GuiInventoryItem[] _guiHotBarInventoryItems;
        private GuiEntityModelView _playerEntityModelView;

        public GuiPlayerInventoryDialog(Player player, Inventory inventory)
        {
            Player = player;
            Inventory = inventory;

            // Subscribe to events

            _guiHotBarInventoryItems = new GuiInventoryItem[inventory?.SlotCount ?? 0];

            if(_guiHotBarInventoryItems.Length != 46) throw new ArgumentOutOfRangeException(nameof(inventory), inventory?.SlotCount ?? 0, "Expected player inventory containing 46 slots.");

            ContentContainer.Background = GuiTextures.InventoryPlayerBackground;
            ContentContainer.Width = ContentContainer.MinWidth = ContentContainer.MaxWidth = 176;
            ContentContainer.Height = ContentContainer.MinHeight = ContentContainer.MaxHeight = 166;
            ContentContainer.AutoSizeMode = AutoSizeMode.None;

            ContentContainer.AddChild(_playerEntityModelView = new GuiEntityModelView(player)
            {
                Margin = new Thickness(26, 8, 0, 0),
                Width = 49,
                Height = 70
            });

            int x = 0, y = 141;

            for (var i = 0; i < _guiHotBarInventoryItems.Length; i++)
            {
                _guiHotBarInventoryItems[i] = new GuiInventoryItem()
                {
                    Item = Inventory[i],
                    HighlightedBackground = new Microsoft.Xna.Framework.Color(1.0f, 1.0f, 1.0f, 0.1f)
                };
                
                if (i == 9)
                {
                    // Hotbar
                    y = 83;
                }
                else if (i == 17)
                {
                    // Main inv row 1
                    y = 338;
                }
                else if (i <= 26)
                {
                    // Main inv row 2
                    y = 396;
                }
                else if (i <= 35)
                {
                    // Main inv row 3
                    y = 396;
                }

                if (i >= 0 && i <= 35)
                {
                    _guiHotBarInventoryItems[i].Margin = new Thickness(x, y, 0, 0);

                    ContentContainer.AddChild(_guiHotBarInventoryItems[i]);

                    x = (i % 9 == 0) ? 0 : x + _guiHotBarInventoryItems[i].Width;
                    if (i == 9 || i == 18 || i == 27)
                    {
                        y += _guiHotBarInventoryItems[i].Height;
                    }
                }
            }
        }

        protected override void OnInit(IGuiRenderer renderer)
        {

            base.OnInit(renderer);
        }

    }
}
