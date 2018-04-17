using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.Gamestates;
using Alex.GameStates.Gui.Elements;
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

        private GuiTextElement  _headerTitle;
        private GuiBeaconButton _headerBackButton;

        public GuiStateBase() : base(Alex.Instance)
        {
            Gui = new GuiScreen(Alex)
            {
                DefaultBackgroundTexture = GuiTextures.OptionsBackground,
                BackgroundRepeatMode = TextureRepeatMode.Tile,
                BackgroundOverlayColor = new Color(Color.Black, 0.25f),
                BackgroundScale = new Vector2(2f, 2f)
            };

            Gui.AddChild(Header = new GuiContainer()
            {
                Height              = 42,
                VerticalAlignment   = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.FillParent,
            });

            Header.AddChild(_headerBackButton = new GuiBackButton()
            {
                VerticalAlignment   = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
            });

            Header.AddChild(_headerTitle = new GuiTextElement()
            {
                Text      = Title,
                TextColor = TextColor.White,
                Scale     = 1.5f,

                VerticalAlignment   = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            Gui.AddChild(Body = new GuiStackContainer()
            {
                Y = Header.Height + 10,

                HorizontalAlignment = HorizontalAlignment.FillParent,
                HorizontalContentAlignment = HorizontalAlignment.Center
            });
        }

        protected void AddGuiElement(GuiElement element)
        {
            Body.AddChild(element);
        }
    }
}
