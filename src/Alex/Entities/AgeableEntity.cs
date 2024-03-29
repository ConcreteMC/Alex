using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

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

			if (entry.Index == 16 && entry is MetadataBool val)
			{
				IsBaby = val.Value;
			}
		}
	}
}