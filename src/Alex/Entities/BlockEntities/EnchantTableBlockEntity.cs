using Alex.Common.Resources;
using Alex.Interfaces.Resources;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.BlockEntities
{
	public class EnchantTableBlockEntity : BlockEntity
	{
		public EnchantTableBlockEntity(World level) : base(level)
		{
			Type = new ResourceLocation("minecraft:enchanttable");
			Width = 1f;
			Height = 1f;

			Offset = new Vector3(0.5f, 0f, 0.5f);

			HideNameTag = true;
			IsAlwaysShowName = false;
			AnimationController.Enabled = true;
		}
	}
}