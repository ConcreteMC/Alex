using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class CaveSpider : Spider
	{
		public CaveSpider(World level) : base(level)
		{
			EntityTypeId = 40;
			JavaEntityId = 59;
			Height = 0.5;
			Width = 0.7;
		}
	}
}
