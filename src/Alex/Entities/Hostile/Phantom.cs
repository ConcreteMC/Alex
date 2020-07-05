using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Phantom : Flying
	{
		private const double BoundingWidth = 0.9;
		private const double BoundingHeight = 0.5;
		
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
				//Scale = value;

				Width = BoundingWidth + 0.2 * value;
				Height = BoundingHeight + 0.1 * value;

				Scale = (float) (Width / BoundingWidth);
			}
		}
		
		/// <inheritdoc />
		public Phantom(World level) : base(EntityType.Phantom, level)
		{
			
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