using System;
using MiNET.Utils.Skins;

namespace Alex.Utils
{
	public class SkinAnimation
	{
		public string Image { get; set; }
		public int ImageWidth { get; set; }
		public int ImageHeight { get; set; }
		public float FrameCount { get; set; }
		public int Type { get; set; } // description above

		public SkinAnimation(Animation animation)
		{
			Image = Convert.ToBase64String(animation.Image);
			ImageWidth = animation.ImageWidth;
			ImageHeight = animation.ImageHeight;
			FrameCount = animation.FrameCount;
			Type = animation.Type;
		}
	}
}