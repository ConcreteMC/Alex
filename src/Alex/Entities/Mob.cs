using Alex.MoLang.Attributes;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;
using MiNET.Utils.Metadata;
using MetadataByte = Alex.Networking.Java.Packets.Play.MetadataByte;

namespace Alex.Entities
{
	public class Mob : Insentient
	{
		public Mob(World level) : base(level)
		{
			Width = 0.6;
			Height = 1.80;
		}

		[MoProperty("variant")]
		public int Variant { get; set; } = 0;

		/// <inheritdoc />
		protected override bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			if (flag == MiNET.Entities.Entity.MetadataFlags.Variant && entry is MetadataInt mti)
			{
				Variant = mti.Value;

				return true;
			}
			
			return base.HandleMetadata(flag, entry);
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
