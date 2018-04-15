using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Icons;
using Alex.API.Gui.Rendering;
using Alex.GameStates.Gui.Common;
using Alex.GameStates.Gui.Elements;
using Alex.Graphics.Gui.Elements;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.GameStates.Gui.MainMenu
{
    public class MultiplayerServerSelectionState : ListSelectionStateBase<GuiServerListEntryElement>
    {

        public MultiplayerServerSelectionState() : base()
        {
	        Title = "Multiplayer";
            AddItem(new GuiServerListEntryElement("Localhost", "localhost:25565"));
        }

    }

    public class GuiServerListEntryElement : GuiContainer
    {
        private const int ServerIconSize = 32;

        public string ServerName { get;set; }
        public string ServerAddress { get; set; }

        public Texture2D ServerIcon { get; private set; }
        public string RawMotd { get; private set; }

        public byte PingQuality { get; private set; }

        public bool IsPingPending { get; private set; }


        private GuiTextureElement _serverIcon;
        private GuiStackContainer _textWrapper;
        private GuiConnectionPingIcon _pingStatus;
        
        private GuiTextElement _serverName;
        private GuiTextElement _serverMotd;

        public GuiServerListEntryElement(string serverName, string serverAddress)
        {
            ServerName = serverName;
            ServerAddress = serverAddress;

	        HorizontalAlignment = HorizontalAlignment.Left;
	        VerticalAlignment = VerticalAlignment.Top;

            AddChild( _serverIcon = new GuiTextureElement()
            {
                Width = ServerIconSize,
                Height = ServerIconSize,

                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,

                DefaultBackgroundTexture = GuiTextures.DefaultServerIcon
            });

            AddChild(_pingStatus = new GuiConnectionPingIcon()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top
            });

            AddChild( _textWrapper = new GuiStackContainer()
            {
                LayoutOffsetX = ServerIconSize + 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top
            });

            _textWrapper.AddChild(_serverName = new GuiTextElement()
            {
                Text = ServerName
            });
            _textWrapper.AddChild(_serverMotd = new GuiTextElement()
            {
				Text = "Pinging server..."
            });


        }

        private void ServerPingCallback(string rawMotd, long pingMs)
        {
            RawMotd = rawMotd;
            
            _serverMotd.Text = rawMotd;
        }
    }
}
