using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Horse : AbstractHorse
	{
		public int Variant { get; set; } = 0;
		public Horse(World level) : base((EntityType)23, level)
		{
			JavaEntityId = 100;
			Height = 1.6;
			Width = 1.396484;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 18 && entry is MetadataVarInt varInt)
			{
				Variant = varInt.Value;
			}
		}
	}
}
