using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class AbstractVillager : PassiveMob
	{
		/// <inheritdoc />
		public AbstractVillager(World level) : base(level)
		{
			Height = 1.95;
			Width = 0.6;
		}
	}
	
	public class Villager : AbstractVillager
	{
		public Villager(World level) : base(level)
		{
			
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 17 && entry is MetadataVillagerData villagerData)
			{
				string texture = villagerData.Profession.ToString().ToLower();
				//TryUpdateTexture("minecraft:villager", texture);
			}
		}
	}
}
