using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Villager : PassiveMob
	{
		public Villager(World level) : base((EntityType)15, level)
		{
			JavaEntityId = 120;
			Height = 1.95;
			Width = 0.6;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 17 && entry is MetadataVillagerData villagerData)
			{
				string texture = villagerData.Profession.ToString().ToLower();
				TryUpdateTexture("minecraft:villager", texture);
			}
		}
	}
}
