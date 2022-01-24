using Alex.Common.Graphics.GpuResources;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Common.Utils
{
	public class Skin
	{
		public bool Slim { get; set; } = false;
		public Texture2D Texture { get; set; } = null;
		public string Url { get; set; } = null;
	}
}