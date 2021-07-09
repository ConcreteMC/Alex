using Alex.Common.Gui.Elements.Icons;
using Alex.Common.Utils;
using RocketUI;

namespace Alex.Gamestates.InGame
{
    public class PlayerListItemElement : RocketControl
    {
        private GuiConnectionPingIcon _pingIcon;
        private PlayerListItem _playerListItem;
        public PlayerListItemElement(PlayerListItem item)
        {
            _playerListItem = item;
            
            AddChild(
                new TextElement()
                {
                    Text = item.Username.Replace("\n", "").Trim(),
                    Margin = new Thickness(2),
                    Anchor = Alignment.TopLeft,
                    //Enabled = false,
                    Padding = new Thickness(5, 5)
                });

            _pingIcon = new GuiConnectionPingIcon
            {
                Anchor = Alignment.MiddleRight, 
                ShowPlayerCount = false, 
                Margin = new Thickness(0, 0, 8, 0),
                IsVisible = false
            };
            _pingIcon.SetPing(item.Ping);

            AddChild(_pingIcon);

            //_ping = ping;
            AutoSizeMode = AutoSizeMode.GrowOnly;
        }

        public void SetPingVisible(bool visible)
        {
            _pingIcon.IsVisible = visible;
        }
        
        /// <inheritdoc />
        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            
            _pingIcon.SetPing(_playerListItem.Ping);
            if (_playerListItem.Ping >= 0)
                SetPingVisible(true);
        }
    }
}