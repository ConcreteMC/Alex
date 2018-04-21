using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;

namespace Alex.GameStates.Gui.Common
{
    public class GuiStateBase : GameState
    {
        public const int ListItemMinWidth = 356;

        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                if(_headerTitle != null)
                    _headerTitle.Text = value;
            }
        }

        protected GuiContainer           Header { get; }
        protected GuiStackContainer Body { get; }
        protected GuiMultiStackContainer Footer { get; }

        private GuiTextElement  _headerTitle;
        private GuiButton _headerBackButton;

        public GuiStateBase() : base(Alex.Instance)
        {
            Gui = new GuiScreen(Alex)
            {
                DefaultBackgroundTexture = GuiTextures.OptionsBackground,
                BackgroundRepeatMode = TextureRepeatMode.Tile,
                BackgroundOverlayColor = new Color(Color.Black, 0.65f),
                BackgroundScale = new Vector2(2f, 2f)
            };

            Gui.AddChild(Header = new GuiContainer()
            {
                Height              = 32,
                Padding = new Thickness(3),

                Anchor = Alignment.TopFill,
            });

            Header.AddChild(_headerBackButton = new GuiBackButton()
            {
                Anchor = Alignment.TopLeft,
            });

            Header.AddChild(_headerTitle = new GuiTextElement()
            {
                Text      = Title,
                TextColor = TextColor.White,
                Scale     = 1.5f,
                
                Anchor = Alignment.MiddleCenter,
            });

            Gui.AddChild(Footer = new GuiMultiStackContainer(row =>
                         {
                             //row.Anchor = Alignment.TopFill;
                             //row.Orientation = Orientation.Horizontal;
                             row.ChildAnchor = Alignment.TopFill;
                             row.Margin = new Thickness(3);
                             row.Width = GuiStateBase.ListItemMinWidth;
                             row.MaxWidth = GuiStateBase.ListItemMinWidth;
                         })
            {
                Height  = 64,
                Padding = new Thickness(3),

                Orientation = Orientation.Vertical,
                Anchor = Alignment.BottomFill,
                ChildAnchor = Alignment.TopCenter
            });

	        Gui.AddChild(Body = new GuiStackContainer()
	        {
		        Margin = new Thickness(0, Header.Height, 0, Footer.Height),

		        Anchor = Alignment.Fill,
		        ChildAnchor = Alignment.MiddleCenter,
		        //HorizontalContentAlignment = HorizontalAlignment.Center
	        });
		}

		protected void AddGuiElement(GuiElement element)
        {
            Body.AddChild(element);
        }
    }
}
