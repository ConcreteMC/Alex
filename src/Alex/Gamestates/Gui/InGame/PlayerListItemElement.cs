using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using RocketUI;

namespace Alex.GameStates.Gui.InGame
{
    public class PlayerListItemElement : GuiControl
    {
        public PlayerListItemElement(string username)
        {
            AddChild(new GuiTextElement()
            {
                Text = username,
                Margin = new Thickness(2),
                Anchor = Alignment.TopCenter,
                Enabled = false,
                Padding = new Thickness(5,5)
            });

            AutoSizeMode = AutoSizeMode.GrowOnly;
        }
    }
}