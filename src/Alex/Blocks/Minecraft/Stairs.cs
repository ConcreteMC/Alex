using System;
using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib.Json;
using NLog;

namespace Alex.Blocks.Minecraft
{
    public class Stairs : Block
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Stairs));
        
        public Stairs(uint baseId) : base(baseId)
        {
            Solid = true;
            Transparent = true;
            IsReplacible = false;
            RequiresUpdate = true;
        }

        public Stairs()
        {
            Solid = true;
            Transparent = true;
            IsReplacible = false;
            RequiresUpdate = true;
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
            var blockState = world.GetBlockState(updatedBlock);
            if (!(blockState?.Block is Stairs)) {return false;}

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