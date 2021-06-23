using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Entities.BlockEntities;

namespace Alex.Blocks.Minecraft
{
	public class Bed : Block
	{
		public static readonly PropertyBool OCCUPIED = new PropertyBool("occupied", "true", "false");
		public static readonly PropertyBool PART = new PropertyBool("part", "foot", "head");
		
		public BedBlockEntity.BedColor Variant { get; }
		public bool IsFoot => BlockState.GetTypedValue(PART);
		public Bed(BedBlockEntity.BedColor variant)
		{
			Variant = variant;
			
			Transparent = true;
			CanInteract = true;
			BlockMaterial = Material.Wood;
		}

		/// <inheritdoc />
		public override bool TryGetStateProperty(string prop, out StateProperty stateProperty)
		{
			switch (prop)
			{
				case "occupied":
					stateProperty = OCCUPIED;
					return true;
				case "part":
					stateProperty = PART;
					return true;
			}
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}