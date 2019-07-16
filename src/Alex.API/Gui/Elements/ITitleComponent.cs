using Alex.API.Utils;

namespace Alex.API.Gui.Elements
{
    public interface ITitleComponent
    {
        void Show();
        void Hide();
        void Reset();
        void SetSubtitle(ChatObject value);
        void SetTimes(int fadeIn, int stay, int fadeOut);
        void SetTitle(ChatObject value);
    }
}