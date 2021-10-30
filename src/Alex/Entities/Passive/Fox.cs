using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Fox : PassiveMob
	{
		public bool IsPouncing { get; set; }
		public bool IsFaceplanted { get; set; }
		public bool IsDefending { get; set; }
		
		/// <inheritdoc />
		public Fox(World level) : base(level)
		{
			Width = 0.6;
			Height = 0.7;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 17 && entry is MetadataVarInt variant)
			{
				Variant = variant.Value;
				//TryUpdateTexture("minecraft:fox", variant.Value == 0 ? "red" : "arctic");
			}
			else if (entry.Index == 18 && entry is MetadataByte mdb)
			{
				IsSitting = (mdb.Value & 0x01) != 0;
				IsSneaking = (mdb.Value & 0x04) != 0;
				IsInterested = (mdb.Value & 0x08) != 0;
				IsPouncing = (mdb.Value & 0x10) != 0;
				IsSleeping = (mdb.Value & 0x20) != 0;
				IsFaceplanted = (mdb.Value & 0x40) != 0;
				IsDefending = (mdb.Value & 0x80) != 0;
			}
		}
	}
}