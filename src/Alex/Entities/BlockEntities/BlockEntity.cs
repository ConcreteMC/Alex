using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
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
		public BlockEntity(World level, Block block) : base(level)
		{
			HasPhysics = false;
			IsAffectedByGravity = false;
			Block = block;
			DoRotationCalculations = false;

			//base.Movement.InterpolatedMovement = false;
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

		protected Vector3 Offset { get; set; } = new Vector3(0.5f, 0f, 0.5f);
		
		/// <inheritdoc />
		public override PlayerLocation KnownPosition
		{
			get => base.KnownPosition;
			set
			{
				base.KnownPosition = value;
				//base.RenderLocation = value;
			}
		}

		public virtual void Render2D(IRenderArgs args){}

		internal override PlayerLocation RenderLocation
		{
			get => base.RenderLocation + Offset;
			set => base.RenderLocation = value;
		}

		public virtual void SetData(byte action, NbtCompound compound)
		{
			//throw new System.NotImplementedException();
		}
	}
}