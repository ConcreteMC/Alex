using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using RocketUI;
using FontStyle = Alex.API.Graphics.Typography.FontStyle;

namespace Alex.Gui.Elements.Inventory
{
    public class InventoryContainerItem : GuiControl
    {
        public const int ItemWidth = 18;
        
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(InventoryContainerItem));
        
        //private Item _item;
       // protected TextureElement TextureElement { get; }
       private GuiItem GuiItem        { get; }
        public int     InventoryIndex { get; set; } = 0;
        public int     InventoryId    { get; set; } = 0;
        
        private TextElement _counTextElement;
        public InventoryContainerItem()
        {
            SetFixedSize(16, 16);
            Padding = new Thickness(0);

           // AddChild(TextureElement = new TextureElement()
          //  {
          //      Anchor = Alignment.Fill
          //  });
          
          AddChild(GuiItem = new GuiItem()
          {
              Anchor = Alignment.MiddleCenter,
              Height = 18,
              Width = 18,
          });
            
           /* AddChild(_counTextElement = new TextElement()
            {
                TextColor = TextColor.White,
                Anchor = Alignment.BottomRight,
                Text = "",
                Scale = 0.75f,
                Margin = new Thickness(0, 0, 1, 1),
                FontStyle = FontStyle.DropShadow,
                CanHighlight = false,
                CanFocus = false
            });*/
           GuiItem.AddChild(_counTextElement = new TextElement()
           {
               TextColor = TextColor.White,
               Anchor = Alignment.BottomRight,
               Text = "",
               Scale = 0.75f,
               Margin = new Thickness(0, 0, 1, 1),
               FontStyle = FontStyle.DropShadow,
               //CanHighlight = false,
              // CanFocus = false
           });
        }
        
        public bool ShowCount
        {
            get
            {
                return _counTextElement.IsVisible;
            }
            set
            {
                _counTextElement.IsVisible = value;
            }
        }
        
        public Item Item
        {
            get => GuiItem.Item;
            set
            {
                GuiItem.Item = value;//.Clone();
                
               // GuiItem.Item = _item;
                
                if (value == null || value is ItemAir || value.Count == 0 || value.Id <= 0)
                {
                 //   TextureElement.IsVisible = false;
                    ShowCount = false;
                    return;
                }

                if (value.Count > 0)
                {
                    _counTextElement.Text = value.Count.ToString();
                    ShowCount = true;
                }
                else
                {
                    _counTextElement.Text = "";
                }
            }
        }

        private bool _showTooltip = false;
        private bool _cursorInContainer = false;
        protected override void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
        {
            if (_cursorInContainer)
            {
                _showTooltip = true;
            }
            else
            {
                _showTooltip = false;
            }
            // TextOverlay.RenderPosition = RenderPosition;
            
            base.OnCursorMove(cursorPosition, previousCursorPosition, isCursorDown);
        }

        protected override void OnCursorEnter(Point cursorPosition)
        {
            _showTooltip = true;
            _cursorInContainer = true;
          //  AddChild(TextOverlay);
            
            base.OnCursorEnter(cursorPosition);
        }

        protected override void OnCursorLeave(Point cursorPosition)
        {
            base.OnCursorLeave(cursorPosition);
            _cursorInContainer = false;
            _showTooltip = false;
            
          //  RemoveChild(TextOverlay);
        }
    }
}