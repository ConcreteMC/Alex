using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Icons;
using Alex.API.Gui.Graphics;
using RocketUI;

namespace Alex.Gamestates.InGame
{
    public class PlayerListItemElement : GuiControl
    {
        private GuiConnectionPingIcon _pingElement = null;
        private int                   _ping        = 0;
        public PlayerListItemElement(string username, int? ping)
        {
            AddChild(new GuiTextElement()
            {
                Text = username.Replace("\n", "").Trim(),
                Margin = new Thickness(2),
                Anchor = Alignment.TopLeft,
                //Enabled = false,
                Padding = new Thickness(5,5)
            });

            if (ping.HasValue)
            {
                AddChild(_pingElement = new GuiConnectionPingIcon
                {
                    Anchor = Alignment.MiddleRight, 
                    ShowPlayerCount = false,
                    Margin = new Thickness(0, 0, 8, 0)
                });
                
                _ping = ping.Value;
            }

            AutoSizeMode = AutoSizeMode.GrowOnly;
        }

        /// <inheritdoc />
        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);

            _pingElement?.SetPing(_ping);
        }
    }
}