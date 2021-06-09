namespace Alex.Common.Gui.Elements
{
    public interface ITitleComponent
    {
        void Show();
        void Hide();
        void Reset();
        void SetSubtitle(string value);
        void SetTimes(int fadeIn, int stay, int fadeOut);
        void SetTitle(string value);
    }
}