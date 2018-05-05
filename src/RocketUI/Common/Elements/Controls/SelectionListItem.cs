namespace RocketUI.Elements.Controls
{
    public class SelectionListItem : Control
    {
        internal SelectionList List { get; set; }

        public SelectionListItem()
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