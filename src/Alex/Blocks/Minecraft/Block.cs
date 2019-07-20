using Alex.API.Blocks;
using Alex.API.Blocks.State;
using Alex.API.Items;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Entities;
using Alex.ResourcePackLib.Json;
using Alex.Utils;
using Microsoft.Xna.Framework;
using MiNET.Blocks;
using MiNET.Items;
using NLog;
using ItemBlock = Alex.Items.ItemBlock;
using ItemType = Alex.API.Utils.ItemType;

namespace Alex.Blocks.Minecraft
{
	public class Block : IBlock
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Block));

		public bool Solid { get; set; }
		public bool Transparent { get; set; }
		public bool Animated { get; set; } = false;
		public bool Renderable { get; set; }
		public bool HasHitbox { get; set; }
		public bool IsBlockNormalCube { get; set; } = false;
		public bool IsFullCube { get; set; } = true;
		public bool IsFullBlock { get; set; } = true;

		public bool RandomTicked { get; set; } = false;
		public bool IsReplacible { get; set; } = false;
		public bool RequiresUpdate { get; set; } = false;

		public float Drag { get; set; }
		public string Name { get; set; }

		public double AmbientOcclusionLightValue { get; set; } = 1.0;
	    public int LightValue { get; set; } = 0;
	    public int LightOpacity { get; set; } = 255;

		//public BlockModel BlockModel { get; set; }
		public IBlockState BlockState { get; set; }
		public bool IsWater { get; set; } = false;
		public bool IsSourceBlock { get; set; } = false;
		public float Hardness { get; set; }

        private IMaterial _material;

		public IMaterial BlockMaterial
		{
			get { return _material; }
			set
			{
				IMaterial newValue = value;
			//	Solid = newValue.IsSolid();
			//	IsReplacible = newValue.IsReplaceable();

				_material = newValue;
			}
		}

		public BlockCoordinates Coordinates { get; set; }
		protected Block(int blockId, byte metadata) : this(BlockFactory.GetBlockStateID(blockId, metadata))
	    {
		    
	    }

	    public Block(uint blockStateId)
	    {
		   //BlockStateID = blockStateId;
			BlockMaterial = new Material(MapColor.STONE);

			Solid = true;
		    Transparent = false;
		    Renderable = true;
		    HasHitbox = true;
		}

		protected Block(string blockName)
		{
		//	BlockStateID = blockStateId;
			BlockMaterial = new Material(MapColor.STONE);

			Solid = true;
			Transparent = false;
			Renderable = true;
			HasHitbox = true;
		}

		protected Block()
		{
			BlockMaterial = new Material(MapColor.STONE);

			Solid = true;
			Transparent = false;
			Renderable = true;
			HasHitbox = true;
		}

		public Microsoft.Xna.Framework.BoundingBox GetBoundingBox(Vector3 blockPosition)
	    {
			if (BlockState == null)
				return new Microsoft.Xna.Framework.BoundingBox(blockPosition, blockPosition + Vector3.One);

		    return BlockState.Model.GetBoundingBox(blockPosition, this);
		}

		public virtual IBlockState BlockPlaced(IWorld world, IBlockState state, BlockCoordinates position)
		{
			return state;
			/*if (BlockState is BlockState s)
			{
				if (s.IsMultiPart)
				{
					BlockStateResource blockStateResource;

					if (Alex.Instance.Resources.ResourcePack.BlockStates.TryGetValue(s.Name, out blockStateResource))
					{
						BlockState.Model = new CachedResourcePackModel(Alex.Instance.Resources,
							MultiPartModels.GetBlockStateModels(world, position, s.VariantMapper.GetDefaultState(), blockStateResource));
						world.SetBlockState(position.X, position.Y, position.Z, BlockState);
					}
				}
			}*/
		}

		public virtual bool Tick(IWorld world, Vector3 position)
		{
			return false;
		}

		public virtual void Interact(IWorld world, BlockCoordinates position, BlockFace face, Entity sourceEntity)
		{

		}

		public virtual void BlockUpdate(IWorld world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			
		}

		public virtual IItem[] GetDrops(IItem tool)
		{
			return new IItem[] { new ItemBlock(BlockState) { Count = 1 } };
		}

        public double GetBreakTime(IItem miningTool)
		{
			double secondsForBreak = Hardness;
			bool isHarvestable = GetDrops(miningTool)?.Length > 0;
			if (isHarvestable)
			{
				secondsForBreak *= 1.5;
			}
			else
			{
				secondsForBreak *= 5;
			}
			if (secondsForBreak == 0D)
			{
				secondsForBreak = 0.05;
			}

			int tierMultiplier = 1;
			switch (miningTool.Material)
			{
				case ItemMaterial.Wood:
					tierMultiplier = 2;
					break;
				case ItemMaterial.Stone:
					tierMultiplier = 4;
					break;
				case ItemMaterial.Gold:
					tierMultiplier = 12;
					break;
				case ItemMaterial.Iron:
					tierMultiplier = 6;
					break;
				case ItemMaterial.Diamond:
					tierMultiplier = 8;
					break;
			}

			if (isHarvestable)
			{
				switch (miningTool.ItemType)
				{
					case ItemType.Shears:
						if (this is Wool)
						{
							return secondsForBreak / 5;
						}
						else if (this is Leaves || this is AcaciaLeaves || this is Cobweb)
						{
							return secondsForBreak / 15;
						}
						break;
					case ItemType.Sword:
						if (this is Cobweb)
						{
							return secondsForBreak / 15;
						}
						return secondsForBreak / 1.5;
					case ItemType.Shovel:
					case ItemType.Axe:
					case ItemType.PickAxe:
					case ItemType.Hoe:
						return secondsForBreak / tierMultiplier;
				}
			}

			return secondsForBreak;
		}

        public string DisplayName { get; set; } = null;
	    public override string ToString()
	    {
		    return DisplayName ?? GetType().Name;
	    }

		public virtual IBlockState GetDefaultState()
		{
			IBlockState r = null;
			if (BlockState is BlockState s)
			{
				r = s.VariantMapper.GetDefaultState();
			}

			if (r == null)
				return new BlockState()
				{

				};

			return r;
		}
	}
}