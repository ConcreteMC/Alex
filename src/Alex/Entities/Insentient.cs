using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Utils;
using MetadataByte = Alex.Networking.Java.Packets.Play.MetadataByte;

namespace Alex.Entities
{
	public abstract class Insentient : LivingEntity
	{
		/// <inheritdoc />
		protected Insentient(int entityTypeId, World level, NetworkProvider network) : base(entityTypeId, level, network) { }

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 14 && entry is MetadataByte bitMask)
			{
				//NoAi = ((bitMask.Value & 0x01) != 0);

				if ((bitMask.Value & 0x02) != 0) //Left Handed
				{
					IsLeftHanded = true;
				}
				else
				{
					IsLeftHanded = false;
				}
			}
		}
	}
}