using System;
using Alex.API.Blocks;
using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib.Json;
using Microsoft.Xna.Framework;
using NLog;
using MathF = System.MathF;

namespace Alex.Blocks.Minecraft
{
    public class WoodStairs : Stairs
    {
        public WoodStairs() : base()
        {
            BlockMaterial = Material.Wood;
        }
    }

    public class Stairs : Block
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Stairs));
        
        public Stairs(uint baseId) : base(baseId)
        {
            Solid = true;
            Transparent = true;
            IsReplacible = false;
            RequiresUpdate = true;

            BlockMaterial = Material.Rock;
        }

        public Stairs()
        {
            Solid = true;
            Transparent = true;
            IsReplacible = false;
            RequiresUpdate = true;
            
            BlockMaterial = Material.Rock;
        }

        public override double GetHeight(Vector3 relative)
        {
            var half = GetHalf(BlockState);

            if (half == "top")
                return 1d;

            //var shape = GetShape(BlockState);
            var facing = GetFacing(BlockState);
           // var a = facing.GetVector3() / 2f;

           // relative += new Vector3(0.5f, 0.5f, 0.5f);

           switch (facing)
            {
                case BlockFace.Down:
                    break;
                case BlockFace.Up:
                    break;
                case BlockFace.East:
                    if (relative.X >= 0.5f)
                        return 1f;
                    return 0.5f;
                    break;
                case BlockFace.West:
                    if (relative.X <= 0.5f)
                        return 1f;
                    return 0.5f;
                    break;
                case BlockFace.North:

                    if (relative.Z <= 0.5f)
                        return 1f;
                    
                    return 0.5f;
                    break;
                case BlockFace.South:
                    if (relative.Z >= 0.5f)
                        return 1f;
                    return 0.5f;
                    break;
                case BlockFace.None:
                    break;
            }
            //Vector3.Round(relative);
           // if (MathF.Round(relative.X) )
            
            //if (relative.X > a.X)
            
            return base.GetHeight(relative);
        }

        protected static BlockFace GetFacing(IBlockState state)
        {
            if (state.TryGetValue("facing", out string facingValue) &&
                Enum.TryParse<BlockFace>(facingValue, true, out BlockFace face))
            {
                return face;
            }

            return BlockFace.None;
        }
        
        protected static string GetHalf(IBlockState state)
        {
            if (state.TryGetValue("half", out string facingValue))
            {
                return facingValue;
            }

            return string.Empty;
        }

        protected static string GetShape(IBlockState state)
        {
            if (state.TryGetValue("shape", out string facingValue))
            {
                return facingValue;
            }

            return string.Empty;
        }
        
        private bool UpdateState(IWorld world, IBlockState state, BlockCoordinates position, BlockCoordinates updatedBlock, out IBlockState result)
        {
            result = state;
            var block = world.GetBlock(updatedBlock);
            if (!(block is Stairs)) {return false;}

            var blockState = block.BlockState;
            if (GetHalf(state) != GetHalf(blockState))
                return false;
            
            var facing = GetFacing(state);
            var neighbor = GetFacing(blockState);

            var myShape = GetShape(state);
            var neighborShape = GetShape(blockState);
            
            //if ()
            {
               // if (neighbor == BlockModel.RotateDirection(facing, 1, BlockModel.FACE_ROTATION,
               //         BlockModel.INVALID_FACE_ROTATION) && neighbor != facing && GetHalf(state) == GetHalf(blockState))
                //if (facing == BlockFace.East && updatedBlock == (position + facing.Opposite().GetBlockCoordinates()))

                BlockCoordinates offset1 = facing.GetVector3();
                
                if (neighbor != facing && neighbor != facing.Opposite() && updatedBlock == position + offset1)
                {
                    if (neighbor == BlockModel.RotateDirection(facing, 1, BlockModel.FACE_ROTATION,
                            BlockModel.INVALID_FACE_ROTATION))
                    {
                        if (facing == BlockFace.North || facing == BlockFace.South)
                        {
                            result = state.WithProperty("shape", "inner_right");
                        }
                        else
                        {
                            result = state.WithProperty("shape", "outer_right");
                        }
                        return true;
                    }

                    if (facing == BlockFace.North || facing == BlockFace.South)
                    {
                        result = state.WithProperty("shape", "inner_left");
                    }
                    else
                    {
                        result = state.WithProperty("shape", "outer_left");
                    }

                    return true;
                }
                
                BlockCoordinates offset2 = facing.Opposite().GetVector3();

                if (neighbor != facing && neighbor != facing.Opposite() && updatedBlock == position + offset2)
                {
                    if (neighbor == BlockModel.RotateDirection(facing, 1, BlockModel.FACE_ROTATION,
                            BlockModel.INVALID_FACE_ROTATION))
                    {
                        if (facing == BlockFace.North || facing == BlockFace.South)
                        {
                            result = state.WithProperty("shape", "outer_right");
                        }
                        else
                        {
                            result = state.WithProperty("shape", "inner_right");
                        }
                        return true;
                    }

                    if (facing == BlockFace.North || facing == BlockFace.South)
                    {
                        result = state.WithProperty("shape", "outer_left");
                    }
                    else
                    {
                        result = state.WithProperty("shape", "inner_left");
                    }
                    return true;
                }

                /* if (updatedBlock == (position + facing.Opposite().GetBlockCoordinates()))
                {
                    return state.WithProperty("shape", "outer_left");
                }*/
            }

            return false;
        }
        
        public override void BlockUpdate(IWorld world, BlockCoordinates position, BlockCoordinates updatedBlock)
        {
            if (UpdateState(world, BlockState, position, updatedBlock, out var state))
            {
                world.SetBlockState(position.X, position.Y, position.Z, state);
            }
        }

        public override IBlockState BlockPlaced(IWorld world, IBlockState state, BlockCoordinates position)
        {
            if (UpdateState(world, state, position, position + BlockCoordinates.Forwards, out state)
                || UpdateState(world, state, position, position + BlockCoordinates.Backwards, out state)
                || UpdateState(world, state, position, position + BlockCoordinates.Left, out state)
                || UpdateState(world, state, position, position + BlockCoordinates.Right, out state))
            {
                return state;
            }

            return state;
        }
    }
}