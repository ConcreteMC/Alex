using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using ConcreteMC.MolangSharp.Attributes;
using MiNET.Utils.Metadata;
using MetadataByte = MiNET.Utils.Metadata.MetadataByte;

namespace Alex.Entities.Hostile
{
	public class Slime : HostileMob
	{
		private const double BoundingWidth = 0.51000005;
		private const double BoundingHeight = 0.51000005;

		private int _size;

		public int Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;

				Width = BoundingWidth + 0.2 * value;
				Height = BoundingHeight + 0.1 * value;

				Scale = (float)(Width / BoundingWidth);
			}
		}

		[MoProperty("previous_squish_value")] public float PreviousSquishValue { get; set; } = 0f;

		[MoProperty("current_squish_value")] public float SquishValue { get; set; } = 0f;

		public Slime(World level) : base(level)
		{
			Height = 0.51000005;
			Width = 0.51000005;
		}

		/// <inheritdoc />
		protected override bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			if (entry.Identifier == 16 && entry is MetadataByte mdb)
			{
				Size = mdb.Value;
			}

			return base.HandleMetadata(flag, entry);
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataVarInt mtd)
			{
				Size = mtd.Value;
			}
		}
	}
}