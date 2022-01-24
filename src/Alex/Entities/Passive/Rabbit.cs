using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Rabbit : PassiveMob
	{
		public Rabbit(World level) : base(level)
		{
			Height = 0.5;
			Width = 0.4;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 17 && entry is MetadataVarInt mdv)
			{
				Variant = mdv.Value;
			}
		}
	}
}