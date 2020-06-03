using System;
using Alex.API.Blocks;
using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib.Json;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
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
            Hardness = 2;
            
            BlockMaterial = Material.Stone;
        }

        public Stairs()
        {
            Solid = true;
            Transparent = true;
            IsReplacible = false;
            RequiresUpdate = true;
            Hardness = 2;
            
            BlockMaterial = Material.Stone;
        }

        /// <inheritdoc />
        public override bool ShouldRenderFace(BlockFace face, Block neighbor)
        {
            var facing = GetFacing(BlockState);

            if (facing != face || face == BlockFace.None)
                return true;
            
            return base.ShouldRenderFace(face, neighbor);
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

        protected static BlockFace GetFacing(BlockState state)
        {
            if (state.TryGetValue("facing", out string facingValue) &&
                Enum.TryParse<BlockFace>(facingValue, true, out BlockFace face))
            {
                return face;
            }

            return BlockFace.None;
        }
        
        protected static string GetHalf(BlockState state)
        {
            if (state.TryGetValue("half", out string facingValue))
            {
                return facingValue;
            }

            return string.Empty;
        }

        protected static string GetShape(BlockState state)
        {
            if (state.TryGetValue("shape", out string facingValue))
            {
                return facingValue;
            }

            return string.Empty;
        }
        
        private bool UpdateState(IBlockAccess world, BlockState state, BlockCoordinates position, BlockCoordinates updatedBlock, out BlockState result)
        {
            result = state;
            var block = world.GetBlockState(updatedBlock).Block;
            if (!(block is Stairs)) {return false;}

            var myHalf = GetHalf(state);
            
            var blockState = block.BlockState;
            if (myHalf != GetHalf(blockState))
                return false;
            
            var facing = GetFacing(state);
            var neighbor = GetFacing(blockState);

            var myShape = GetShape(state);
            var neighborShape = GetShape(blockState);

           // int offset = (myHalf == "top") ? -1 : 1;

         //  var innerRight = ""
           if (myHalf == "top")
           {
               
           }
           
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
        
        public override void BlockUpdate(World world, BlockCoordinates position, BlockCoordinates updatedBlock)
        {
            if (UpdateState(world, BlockState, position, updatedBlock, out var state))
            {
                world.SetBlockState(position.X, position.Y, position.Z, state);
            }
        }

        public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
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