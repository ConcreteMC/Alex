using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Bee : PassiveMob
	{
		/// <inheritdoc />
		public Bee(World level) : base(EntityType.Bee, level)
		{
			Width = 0.7;
			Height = 0.6;
		}
		
		public bool HasStung { get; set; }
		public bool HasNectar { get; set; }
		public int AngerTime { get; set; }

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataByte meta)
			{
				IsAngry = (meta.Value & 0x02) != 0;
				HasStung = (meta.Value & 0x04) != 0;
				HasNectar = (meta.Value & 0x08) != 0;
			}

			if (entry.Index == 17 && entry is MetadataVarInt angerTime)
			{
				AngerTime = angerTime.Value;
			}
		}
	}
}