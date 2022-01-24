using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Vehicles
{
	public class MinecartEntity : VehicleEntity
	{
		/// <inheritdoc />
		public MinecartEntity(World level) : base(level) { }

		public int ShakingPower { get; set; } = 0;
		public int ShakingDirection { get; set; } = 0;
		public float ShakingMultiplier { get; set; } = 1f;
		public int CustomBlockId { get; set; } = 0;
		public int CustomBlockYPosition { get; set; } = 6;
		public bool ShowCustomBlock { get; set; } = false;
		
		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);
			
			if (entry is MetadataVarInt mtd)
			{
				switch (entry.Index)
				{
					case 8:
						ShakingPower = mtd.Value;
						break;
					case 9:
						ShakingDirection = mtd.Value;
						break;
					case 11:
						CustomBlockId = mtd.Value;
						break;
					case 12:
						CustomBlockYPosition = mtd.Value;
						break;
				}
			}
			else if (entry is MetadataFloat mfloat && entry.Index == 10)
			{
				ShakingMultiplier = mfloat.Value;
			}
			else if (entry is MetadataBool mBool && entry.Index == 13)
			{
				ShowCustomBlock = mBool.Value;
			}
		}
	}
}