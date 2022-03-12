using Alex.Common.Graphics.Typography;

namespace Alex.ResourcePackLib.Abstraction
{
    public interface IFontSourceProvider
    {
        BitmapFontSource[] FontSources { get; }
    }
}