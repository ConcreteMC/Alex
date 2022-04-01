using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Vex : HostileMob
	{
		public Vex(World level) : base(level)
		{
			Height = 0.8;
			Width = 0.4;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataByte mdb)
			{
				IsAttacking = (mdb.Value & 0x01) != 0;
			}
		}
	}
}