using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiSelectionListItem : GuiControl
    {
        internal GuiSelectionList List { get; set; }

        public GuiSelectionListItem()
        {
            Padding = new Thickness(2);

            FocusOutlineThickness = Thickness.One;
        }

        protected override void OnCursorPressed(Point cursorPosition)
        {
            base.OnCursorPressed(cursorPosition);

            List?.SetSelectedItem(this);
        }
    }
}