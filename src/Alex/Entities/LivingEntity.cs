using Alex.API.Network;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities
{
	public class LivingEntity : Entity
	{
		public bool IsLeftHanded { get; set; } = false;
		
		/// <inheritdoc />
		public LivingEntity(int entityTypeId, World level, NetworkProvider network) : base(
			entityTypeId, level, network)
		{
			
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 8 && entry is MetadataFloat flt)
			{
				HealthManager.Health = flt.Value;
			}
		}
	}
}
