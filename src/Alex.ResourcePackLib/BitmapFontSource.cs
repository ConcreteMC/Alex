using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.ResourcePackLib
{

	public class BitmapFontSource
	{
		public Image<Rgba32> Image { get; }
		public char[] Characters { get; }
		public bool IsAscii { get; }
		public string Name { get; }

		public BitmapFontSource(string name, Image<Rgba32> image, string[] characters, bool isAscii = false)
		{
			Name = name;
			Image = image;
			Characters = characters.SelectMany(x => x.ToCharArray()).ToArray();
			IsAscii = isAscii;
		}

		public BitmapFontSource(string name, Image<Rgba32> image, char unicodeStartChar)
		{
			Name = name;
			Image = image;
			Characters = Enumerable.Range(unicodeStartChar, 256).Select(x => (char) x).ToArray();
		}
	}
}