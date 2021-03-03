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
		protected Insentient(World level) : base(level) { }

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);
		}
	}
}