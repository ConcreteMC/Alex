using RocketUI;

namespace Alex.API.Gui
{
    public interface IFocusableElement : IGuiElement
    {
        [DebuggerVisible] bool Focused { get; set; }
    }
}
