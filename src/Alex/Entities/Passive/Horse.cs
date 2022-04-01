using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Horse : AbstractHorse
	{
		public Horse(World level) : base(level)
		{
			Height = 1.6;
			Width = 1.396484;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 19 && entry is MetadataVarInt varInt)
			{
				Variant = varInt.Value;
			}
		}
	}
}