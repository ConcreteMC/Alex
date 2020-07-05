using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Spider : HostileMob
	{
		/// <inheritdoc />
		public override bool IsWallClimbing
		{
			get
			{
				return base.IsWallClimbing;
			}
			set
			{
				base.IsWallClimbing = value;
				//ModelRenderer.Scale
			}
		}

		public Spider(World level) : base((EntityType)35, level)
		{
			JavaEntityId = 52;
			Height = 0.9;
			Width = 1.4;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 15 && entry is MetadataByte mtd)
			{
				IsWallClimbing = (mtd.Value & 0x01) != 0;
			}
		}
	}
}
