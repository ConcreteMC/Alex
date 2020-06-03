using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using RocketUI;

namespace Alex.Gamestates.InGame
{
    public class PlayerListItemElement : GuiControl
    {
        public PlayerListItemElement(string username)
        {
            AddChild(new GuiTextElement()
            {
                Text = username,
                Margin = new Thickness(2),
                Anchor = Alignment.TopLeft,
                Enabled = false,
                Padding = new Thickness(5,5)
            });

            AutoSizeMode = AutoSizeMode.GrowOnly;
        }
    }
}