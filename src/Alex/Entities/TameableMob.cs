using Alex.Entities.Passive;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities
{
	public class TameableMob : PassiveMob
	{
		/// <inheritdoc />
		protected TameableMob(EntityType type, World level) : base(level)
		{
			
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 17 && entry is MetadataByte meta)
			{
				IsSitting = (meta.Value & 0x01) != 0;
				IsAngry = (meta.Value & 0x02) != 0;
				IsTamed = (meta.Value & 0x04) != 0;
			}
			else if (entry.Index == 18 && entry is MetadataOptUUID ownerId)
			{
				if (ownerId.HasValue && Level.EntityManager.TryGet(ownerId.Value, out var e))
				{
					OwnerEntityId = e.EntityId;
				}
				else
				{
					OwnerEntityId = -1;
				}
			}
		}
	}
}