using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Ocelot : PassiveMob
	{
		public Ocelot(World level) : base((EntityType)22, level)
		{
			JavaEntityId = 98;
			Height = 0.7;
			Width = 0.6;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);
			
			
		}
	}
}
