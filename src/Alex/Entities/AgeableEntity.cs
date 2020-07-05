using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities
{
	public class AgeableEntity : Mob
	{
		/// <inheritdoc />
		public AgeableEntity(int entityTypeId, World level, NetworkProvider network) : base(entityTypeId, level, network) { }

		/// <inheritdoc />
		public AgeableEntity(EntityType mobTypes, World level, NetworkProvider network) : base(mobTypes, level, network) { }

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 15 && entry is MetadataBool val)
			{
				IsBaby = val.Value;
			}
		}
	}
}