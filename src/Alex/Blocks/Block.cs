using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using Alex.API.Blocks;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Entities;
using Alex.Graphics.Models;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.Utils;
using Microsoft.Xna.Framework;
using NLog;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;

namespace Alex.Blocks
{
	public class Block : IBlock
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Block));

		public bool Solid { get; set; }
		public bool Transparent { get; set; }
		public bool Renderable { get; set; }
		public bool HasHitbox { get; set; }
		public bool IsBlockNormalCube { get; set; } = false;
		public bool IsFullCube { get; set; } = true;
		public bool IsFullBlock { get; set; } = true;

		public bool RandomTicked { get; set; } = false;
		public bool IsReplacible { get; set; } = false;

		public float Drag { get; set; }
		public string Name { get; set; }

		public double AmbientOcclusionLightValue { get; set; } = 1.0;
	    public int LightValue { get; set; } = 0;
	    public int LightOpacity { get; set; } = 0;

		//public BlockModel BlockModel { get; set; }
		public IBlockState BlockState { get; set; }
		public bool IsWater { get; set; } = false;
		public bool IsSourceBlock { get; set; } = false;

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

		public virtual void BlockPlaced(IWorld world, BlockCoordinates position)
		{
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