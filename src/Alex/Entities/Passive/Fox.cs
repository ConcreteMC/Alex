using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Fox : PassiveMob
	{
		/// <inheritdoc />
		public Fox(World level) : base(EntityType.Fox, level)
		{
			Width = 0.6;
			Height = 0.7;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataVarInt variant)
			{
				TryUpdateTexture("minecraft:fox", variant.Value == 0 ? "red" : "arctic");
			}
		}
	}
}