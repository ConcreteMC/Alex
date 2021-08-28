using Alex.Blocks.Minecraft;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
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

		protected BlockCoordinates BlockCoordinates => new BlockCoordinates(X, Y, Z);

		private Block _block = null;

		public Block Block
		{
			get
			{
				return _block;
			}
			/*set
			{
				var oldValue = _block;
				_block = value;
				
				if (value == null || !BlockChanged(oldValue, value))
				{
					Level?.EntityManager.RemoveBlockEntity(BlockCoordinates);
				}
			}*/
		}
		
		/// <inheritdoc />
		public BlockEntity(World level) : base(level)
		{
			IsAffectedByGravity = false;
			//Block = block;
			DoRotationCalculations = false;
			AnimationController.Enabled = false;
			//base.Movement.InterpolatedMovement = false;
		}

		public virtual bool SetBlock(Block block)
		{
			var oldValue = _block;
			_block = block;
				
			if (block == null || !BlockChanged(oldValue, block))
			{
				return false;
			}
			
			return true;
		}

		/// <inheritdoc />
		public override bool NoAi
		{
			get
			{
				return true;
			}
		}

		protected virtual bool BlockChanged(Block oldBlock, Block newBlock)
		{
			return true;
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

		public virtual void Render2D(IRenderArgs args){}

		private PlayerLocation GetLocation() => new PlayerLocation(X + Offset.X, Y + Offset.Y, Z + Offset.Z);
		protected Vector3 Rotation { get; set; } = Vector3.Zero;
		
		internal override PlayerLocation RenderLocation
		{
			get => GetLocation();
			set
			{
				//base.RenderLocation = value;
			}
		}

		/// <inheritdoc />
		public override PlayerLocation KnownPosition
		{
			get => GetLocation();
			set
			{
				//base.RenderLocation = value;
			}
		}
		
		/// <inheritdoc />
		public override int Render(IRenderArgs renderArgs, bool useCulling)
		{
			int renderCount = 0;
			var  renderer = ModelRenderer;

			if (!IsInvisible && RenderEntity && renderer != null)
			{
				Vector3 vector = Vector3.Backward;

				var offset = new Vector3(Offset.X, Offset.Y, Offset.Z);

				var rot = Matrix.CreateRotationX(Rotation.X.ToRadians())
				          * Matrix.CreateRotationY(Rotation.Y.ToRadians())
				          * Matrix.CreateRotationZ(Rotation.Z.ToRadians());

				//offset = Vector3.Transform(offset, rot);
				vector = Vector3.Transform(vector, rot);
				
				renderCount += renderer.Render(
					renderArgs,
					Matrix.CreateScale(Scale / 16f) 
					* Matrix.CreateWorld(new PlayerLocation(X + offset.X, Y + offset.Y, Z + offset.Z), vector, Vector3.Up));
			}

			return renderCount;
		}
		
		public virtual void SetData(byte action, NbtCompound compound)
		{
			//throw new System.NotImplementedException();
		}
	}
}