using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Bat : PassiveMob
	{
		public bool IsHanging { get; set; } = false;
		public Bat(World level) : base((EntityType)19, level)
		{
			JavaEntityId = 65;
			Height = 0.9;
			Width = 0.5;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 15 && entry is MetadataByte meta)
			{
				IsHanging = (meta.Value & 0x01) != 0;
			}
		}
	}
}
