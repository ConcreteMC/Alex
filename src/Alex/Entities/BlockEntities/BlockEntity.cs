using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Net;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework;

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

				if (value != null)
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
			
			ReadFrom(compound);
		}
		
		protected virtual void ReadFrom(NbtCompound compound){}

		public virtual void HandleBlockAction(byte actionId, int parameter)
		{
			
		}
		
		/// <inheritdoc />
		public override PlayerLocation KnownPosition
		{
			get => base.KnownPosition + new Vector3(0.5f, 0, 0.5f);
			set => base.KnownPosition = value;
		}

		public virtual void SetData(byte action, NbtCompound compound)
		{
			//throw new System.NotImplementedException();
		}
	}
}