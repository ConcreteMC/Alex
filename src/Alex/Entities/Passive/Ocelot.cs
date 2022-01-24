using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Ocelot : PassiveMob
	{
		public bool IsTrusting { get; set; }

		public Ocelot(World level) : base(level)
		{
			Height = 0.7;
			Width = 0.6;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 17 && entry is MetadataBool mdb)
			{
				IsTrusting = mdb.Value;
			}
		}
	}
}