namespace Alex.API.Gui
{
    public interface IGuiScreen : IGuiElement, IGuiFocusContext
    {

        void UpdateLayout();

    }
}
