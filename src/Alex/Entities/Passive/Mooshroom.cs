using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Mooshroom : PassiveMob
	{
		public Mooshroom(World level) : base(level)
		{
			Height = 1.4;
			Width = 0.9;
		}
		
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 17 && entry is MetadataString variant)
			{
				//TryUpdateTexture("minecraft:mooshroom", variant.Value == "red" ? "default" : "brown");
			}
		}
	}
}
