using System;
using System.Collections.Generic;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Graphics.Models;

using Alex.Utils;
using log4net;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Entities;
using MiNET.Utils;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;

namespace Alex.Blocks
{
	public class Block : IBlock
	{
	    private static readonly ILog Log = LogManager.GetLogger(typeof(Block));
	    
		public uint BlockStateID { get; }

        public int BlockId { get; }
        public byte Metadata { get; set; }
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

	    public double AmbientOcclusionLightValue { get; set; } = 1.0;
	    public int LightValue { get; set; } = 0;
	    public int LightOpacity { get; set; } = 0;

		public BlockModel BlockModel { get; set; }
		public IBlockState BlockState { get; set; }
		protected Block(int blockId, byte metadata) : this(BlockFactory.GetBlockStateID(blockId, metadata))
	    {
		    
	    }

	    public Block(uint blockStateId)
	    {
		    BlockStateID = blockStateId;

		    int blockId;
		    byte meta;

		    BlockFactory.StateIDToRaw(blockStateId, out blockId, out meta);
		    BlockId = blockId;
		    Metadata = meta;

			Solid = true;
		    Transparent = false;
		    Renderable = true;
		    HasHitbox = true;

		    SetColor(TextureSide.All, Color.White);
		}

		public Microsoft.Xna.Framework.BoundingBox GetBoundingBox(Vector3 blockPosition)
	    {
			if (BlockModel == null)
				return new Microsoft.Xna.Framework.BoundingBox(blockPosition, blockPosition + Vector3.One);

		    return BlockModel.GetBoundingBox(blockPosition, this);
		}

        public VertexPositionNormalTextureColor[] GetVertices(Vector3 position, IWorld world)
        {
	        if (BlockModel == null)
				return new VertexPositionNormalTextureColor[0];

			return BlockModel.GetVertices(world, position, this);
        }

	    public void SetColor(TextureSide side, Color color)
        {
            switch (side)
            {
                case TextureSide.Top:
                    TopColor = color;
                    break;
                case TextureSide.Bottom:
                    BottomColor = color;
                    break;
                case TextureSide.Side:
                    SideColor = color;
                    break;
                case TextureSide.All:
                    TopColor = color;
                    BottomColor = color;
                    SideColor = color;
                    break;
            }
        }

		private Dictionary<string, bool> SolidSides { get; set; } = new Dictionary<string, bool>()
		{
			{"north", true },
			{"up", true },
			{"west", true },
			{"east",true },
			{"down",true },
			{"south", true }
		};

		internal void SetSideSolid(string side, bool value)
		{
			if (SolidSides.ContainsKey(side))
				SolidSides[side] = value;
		}

		public bool IsSideSolid(string side)
		{
			if (SolidSides.TryGetValue(side, out bool res))
				return res;

			return true;
		}

		public virtual void BlockPlaced(IWorld world, BlockCoordinates position)
		{

		}

		public virtual bool Tick(IWorld world, Vector3 position)
		{
			return false;
		}

		public virtual void Interact(IWorld world, BlockCoordinates position, BlockFace face, Entity sourceEntity)
		{

		}

		public Color TopColor { get; private set; }
        public Color SideColor { get; private set; }
		public Color BottomColor { get; private set; }

	    public string DisplayName { get; set; } = null;
	    public override string ToString()
	    {
		    return DisplayName ?? GetType().Name;
	    }

		public virtual IBlockState GetDefaultState()
		{
			return BlockState ?? new BlockState()
			{
				//Name = DisplayName,
				ID = BlockStateID
			};
		}

		public static BlockCoordinates GetBlockCoordinatesFromFace(BlockCoordinates position, BlockFace face)
		{
			switch (face) {
				case BlockFace.Down:
					return position + BlockCoordinates.Down;
				case BlockFace.Up:
					return position + BlockCoordinates.Up;
				case BlockFace.East:
					return position + BlockCoordinates.East;
				case BlockFace.West:
					return position + BlockCoordinates.West;
				case BlockFace.North:
					return position + BlockCoordinates.North;
				case BlockFace.South:
					return position + BlockCoordinates.South;
				default:
					return position;
			}
		}

		public BlockFace RotateY(BlockFace v)
		{
			switch (v)
			{
				case BlockFace.North:
					return BlockFace.East;
				case BlockFace.East:
					return BlockFace.South;
				case BlockFace.South:
					return BlockFace.West;
				case BlockFace.West:
					return BlockFace.North;
				default:
					throw new Exception("Unable to get Y-rotated facing of " + this);
			}
		}

		private BlockFace RotateX(BlockFace v)
		{
			switch (v)
			{
				case BlockFace.North:
					return BlockFace.Down;
				case BlockFace.East:
				case BlockFace.West:
				default:
					throw new Exception("Unable to get X-rotated facing of " + this);
				case BlockFace.South:
					return BlockFace.Up;
				case BlockFace.Up:
					return BlockFace.North;
				case BlockFace.Down:
					return BlockFace.South;
			}
		}

		private BlockFace RotateZ(BlockFace v)
		{
			switch (v)
			{
				case BlockFace.East:
					return BlockFace.Down;
				case BlockFace.South:
				default:
					throw new Exception("Unable to get Z-rotated facing of " + this);
				case BlockFace.West:
					return BlockFace.Up;
				case BlockFace.Up:
					return BlockFace.East;
				case BlockFace.Down:
					return BlockFace.West;
			}
		}
	}
}