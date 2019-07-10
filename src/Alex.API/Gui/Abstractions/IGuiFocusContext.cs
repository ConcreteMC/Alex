namespace Alex.API.Gui
{
    public interface IGuiFocusContext
    {
        IGuiControl FocusedControl { get; }
        
        bool Focus(IGuiControl control);
        void ClearFocus(IGuiControl control);

        void HandleContextActive();
        void HandleContextInactive();

    }
}
