using Alex.MoLang.Attributes;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities
{
	public class Mob : Insentient
	{
		public Mob(World level) : base(level)
		{
			Width = 0.6;
			Height = 1.80;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 14 && entry is MetadataByte data)
			{
				NoAi = (data.Value & 0x01) != 0;
				IsLeftHanded = (data.Value & 0x02) != 0;
			}
		}

		public override void OnTick()
		{
			base.OnTick();
		}
	}
}
