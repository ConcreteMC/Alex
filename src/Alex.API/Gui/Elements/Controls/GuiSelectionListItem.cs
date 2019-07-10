using RocketUI;

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

        protected override void OnFocusActivate()
        {
            base.OnFocusActivate();

            List?.SetSelectedItem(this);
        }
        protected override void OnFocusDeactivate()
        {
            base.OnFocusActivate();

            //List?.UnsetSelectedItem(this);
        }
    }
}