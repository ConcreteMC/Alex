using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;

namespace Alex.GameStates.Gui.Common
{
    public class GuiStateBase : GameState
    {
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

        protected GuiContainer    Header { get; }
        protected GuiStackContainer Body { get; }
        protected GuiContainer Footer { get; }

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

            Gui.AddChild(Footer = new GuiContainer()
            {
                Height  = 32,
                Padding = new Thickness(3),

                Anchor = Alignment.BottomFill
            });

	        Gui.AddChild(Body = new GuiStackContainer()
	        {
		        Margin = new Thickness(0, Header.Height, 0, Footer.Height),

		        Anchor = Alignment.FillX,
		        ChildAnchor = Alignment.TopCenter,
		        //HorizontalContentAlignment = HorizontalAlignment.Center
	        });
		}

		protected void AddGuiElement(GuiElement element)
        {
            Body.AddChild(element);
        }
    }
}
