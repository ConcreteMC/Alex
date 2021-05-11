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
		public float ShakeAngle { get; set; } = 0f;

		public byte CollarColor { get; set; } = 0;
		
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
