using Alex.Entities.Passive;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities
{
	public class TameableMob : PassiveMob
	{
		/// <inheritdoc />
		protected TameableMob(EntityType type, World level) : base(type, level)
		{
			
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataByte meta)
			{
				IsSitting = (meta.Value & 0x01) != 0;
				IsAngry = (meta.Value & 0x02) != 0;
				IsTamed = (meta.Value & 0x04) != 0;
			}
		}
	}
}