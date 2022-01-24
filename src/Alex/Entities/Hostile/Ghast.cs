using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Ghast : Flying
	{
		public Ghast(World level) : base(level)
		{
			Height = 4;
			Width = 4;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataBool mdb)
			{
				IsAttacking = mdb.Value;
			}
		}
	}
}