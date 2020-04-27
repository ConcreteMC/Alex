namespace Alex.Blocks.Minecraft
{
	public class Bedrock : Block
	{
		public Bedrock() : base(33)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 60000;
			BlockMaterial = Material.Stone;
		}
	}
}
