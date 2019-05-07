namespace Alex.Blocks.Minecraft
{
	public class Torch : Block
	{
		public Torch(bool wallTorch = false) : base(1040)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			LightValue = 14;
		}
	}
}
