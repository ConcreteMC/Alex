using Alex.MoLang.Attributes;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;
using MiNET.Utils.Metadata;

namespace Alex.Entities.Passive
{
	public class Wolf : TameableMob
	{
		public bool IsBegging { get; set; } = false;

		[MoProperty("shake_angle")]
		public double ShakeAngle { get; set; } = 0d;

		[MoProperty("tail_angle")]
		public double TailAngle { get; set; } = 0d;
		
		[MoProperty("is_shaking_wetness")]
		public bool IsShakingWetness { get; set; } = false;
		
		public byte CollarColor { get; set; } = 0;
		public int AngerTime { get; set; } = 0;
		
		public Wolf(World level) : base((EntityType)14, level)
		{
			Height = 0.85;
			Width = 0.6;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 18 && entry is MetadataBool boolean)
			{
				IsBegging = boolean.Value;
			}
			else if (entry.Index == 20 && entry is MetadataVarInt collarColor)
			{
				CollarColor = (byte) collarColor.Value;
			}
			else if (entry.Index == 21 && entry is MetadataVarInt angerTime)
			{
				AngerTime = angerTime.Value;
			}
		}

		/// <inheritdoc />
		protected override bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			if (flag == MiNET.Entities.Entity.MetadataFlags.Color && entry is MiNET.Utils.Metadata.MetadataByte color)
			{
				CollarColor = color.Value;
				return true;
			}
			
			return base.HandleMetadata(flag, entry);
		}
	}
}
