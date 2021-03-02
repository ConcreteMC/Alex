using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities
{
	public class AgeableEntity : Mob
	{
		/// <inheritdoc />
		public AgeableEntity(World level) : base(level) { }

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