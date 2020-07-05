using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Mooshroom : PassiveMob
	{
		public Mooshroom(World level) : base(EntityType.MushroomCow, level)
		{
			JavaEntityId = 96;
			Height = 1.4;
			Width = 0.9;
		}
		
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataString variant)
			{
				TryUpdateTexture("minecraft:mooshroom", variant.Value == "red" ? "default" : "brown");
			}
		}
	}
}
