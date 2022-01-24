using Alex.MoLang.Attributes;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class MagmaCube : Slime
	{
		public MagmaCube(World level) : base(level)
		{
			Height = 0.51000005;
			Width = 0.51000005;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 15 && entry is MetadataVarInt mtd)
			{
				Size = mtd.Value;
			}
		}
	}
}