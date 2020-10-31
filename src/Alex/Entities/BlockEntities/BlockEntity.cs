using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Net;
using Alex.Worlds;
using fNbt;

namespace Alex.Entities.BlockEntities
{
	public class BlockEntity : Entity
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }

		private Block _block = null;

		public Block Block
		{
			get
			{
				return _block;
			}
			set
			{
				var oldValue = _block;
				_block = value;
				
				BlockChanged(oldValue, value);
			}
		}
		
		/// <inheritdoc />
		public BlockEntity(World level, Block block) : base(-1, level, null)
		{
			IsAffectedByGravity = false;
			Block = block;
		}

		/// <inheritdoc />
		public override bool NoAi
		{
			get
			{
				return true;
			}
		}

		protected virtual void BlockChanged(Block oldBlock, Block newBlock)
		{
			
		}

		public void Read(NbtCompound compound)
		{
			X = compound.Get<NbtInt>("x").Value;
			Y = compound.Get<NbtInt>("y").Value;
			Z = compound.Get<NbtInt>("z").Value;
		}

		public virtual void HandleBlockAction(byte actionId, int parameter)
		{
			
		}
	}
}