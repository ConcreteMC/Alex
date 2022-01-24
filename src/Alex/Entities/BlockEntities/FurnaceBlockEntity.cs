using Alex.Blocks.Minecraft;
using Alex.Items;
using Alex.Networking.Java.Packets.Play;
using Alex.Utils.Inventories;
using Alex.Worlds;
using fNbt;

namespace Alex.Entities.BlockEntities
{
	public class FurnaceBlockEntity : BlockEntity
	{
		public short CookTime { get; set; }
		public short CookTimeTotal { get; set; }
		public short BurnTime { get; set; }
		public short BurnTick { get; set; }

		/// <inheritdoc />
		public FurnaceBlockEntity(World level) : base(level)
		{
			Inventory = new Inventory(3);
		}

		/// <inheritdoc />
		protected override void ReadFrom(NbtCompound compound)
		{
			base.ReadFrom(compound);

			if (compound.TryGet("Items", out NbtList list))
			{
				foreach (var item in list)
				{
					if (item.TagType != NbtTagType.Compound)
						continue;

					NbtCompound itemCompound = (NbtCompound)item;
					var count = itemCompound["Count"].ByteValue;
					var slot = itemCompound["Slot"].ByteValue;
					var id = itemCompound["id"].ShortValue;

					if (itemCompound.TryGet("Damage", out NbtShort damageTag)) { }

					//ItemFactory.
					//var         damage        = itemCompound["Damage"].ShortValue;
				}
			}

			if (compound.TryGet("BurnTime", out NbtShort burnTime))
			{
				BurnTime = burnTime.Value;
			}

			if (compound.TryGet("CookTime", out NbtShort cookTime))
			{
				CookTime = cookTime.Value;
			}

			if (compound.TryGet("CookTimeTotal", out NbtShort cookTimeTotal))
			{
				CookTimeTotal = cookTimeTotal.Value;
			}
		}

		/// <inheritdoc />
		public override void SetData(BlockEntityActionType action, NbtCompound compound)
		{
			base.SetData(action, compound);
		}
	}
}