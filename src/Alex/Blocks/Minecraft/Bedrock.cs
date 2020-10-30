namespace Alex.Blocks.Minecraft
{
	public class Bedrock : Block
	{
		public Bedrock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 60000;
			BlockMaterial = Material.Stone;
		}
	}
}
