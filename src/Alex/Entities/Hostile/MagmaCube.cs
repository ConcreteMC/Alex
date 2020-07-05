using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class MagmaCube : HostileMob
	{
		private const double BoundingWidth  = 0.51000005;
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

				Scale = (float) (Width / BoundingWidth);
			}
		}
		
		public MagmaCube(World level) : base((EntityType)42, level)
		{
			JavaEntityId = 62;
			Height = 0.51000005;
			Width = 0.51000005;
		}
		
		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 15 && entry is MetadataVarInt mtd)
			{
				Size = mtd.Value;
			}
		}
	}
}
