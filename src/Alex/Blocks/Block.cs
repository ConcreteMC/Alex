using System;
using System.Collections.Generic;
using Alex.Blocks.Materials;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Minecraft.Leaves;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Common.Resources;
using Alex.Common.Utils.Noise;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Graphics.Models.Blocks;
using Alex.Interfaces;
using Alex.Items;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;
using NLog;
using ItemMaterial = Alex.Common.Items.ItemMaterial;
using ItemType = Alex.Common.Items.ItemType;
using Wool = Alex.Blocks.Minecraft.Wool;

namespace Alex.Blocks
{
	public class Block : IRegistryEntry<Block>
	{
		public static bool FancyGraphics { get; set; } = true;

		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Block));

		protected static PropertyBool Lit = new PropertyBool("lit", "true", "false");
		protected static PropertyBool WaterLogged = new PropertyBool("waterlogged", "true", "false");

		public bool IsWaterLogged => WaterLogged.GetValue(BlockState); // BlockState.GetTypedValue(WaterLogged);

		private ushort _flags = 0;

		private bool GetFlagBit(int bit) => (_flags & (1 << bit)) != 0;

		private void SetFlagBit(int bit, bool value)
		{
			var mask = (ushort)(1 << bit);

			if (value)
			{
				_flags |= mask;
			}
			else
			{
				_flags = (ushort)(_flags & ~mask);
			}
		}

		public bool Solid { get => GetFlagBit(1); set => SetFlagBit(1, value); }
		public bool Transparent { get => GetFlagBit(2); set => SetFlagBit(2, value); }
		public bool Renderable { get => GetFlagBit(4); set => SetFlagBit(4, value); }
		public bool HasHitbox { get => GetFlagBit(5); set => SetFlagBit(5, value); }
		public virtual bool IsFullCube { get => GetFlagBit(6); set => SetFlagBit(6, value); }

		/// <summary>
		///		If true, the <see cref="BlockPlaced"/> function gets called whenever a block of this type of placed
		/// </summary>
		public bool RequiresUpdate { get => GetFlagBit(9); set => SetFlagBit(9, value); }

		/// <summary>
		///		If true, clicking this block will send the server an interact event
		/// </summary>
		public bool CanInteract { get => GetFlagBit(10); set => SetFlagBit(10, value); }

		/// <summary>
		///		The amount of light this block emits
		/// </summary>
		public virtual byte Luminance { get; set; } = 0;

		/// <summary>
		///		The amount of light this block blocks.
		/// </summary>
		public int Diffusion { get; set; } = 1;

		public BlockState BlockState { get; set; }

		private IMaterial _material = new Material(MapColor.Stone);

		public virtual IMaterial BlockMaterial
		{
			get { return _material; }
			set
			{
				IMaterial newValue = value;

				_material = newValue;
			}
		}

		protected Block()
		{
			Solid = true;
			Transparent = false;
			Renderable = true;
			HasHitbox = true;
			IsFullCube = true;
		}

		public virtual IEnumerable<BoundingBox> GetBoundingBoxes(Vector3 blockPos)
		{
			bool didReturn = false;

			if (BlockState?.VariantMapper?.Model != null)
			{
				foreach (var bb in BlockState.VariantMapper.Model.GetBoundingBoxes(BlockState, blockPos))
				{
					didReturn = true;

					yield return bb;
				}

				if (!didReturn)
					yield return new BoundingBox(blockPos, blockPos + Vector3.One);
			}
			else
			{
				yield return new BoundingBox(blockPos, blockPos + Vector3.One);
			}
		}

		public virtual Vector3 GetOffset(IModule3D noise, BlockCoordinates position)
		{
			return Vector3.Zero;
		}

		public virtual bool CanClimb(BlockFace face)
		{
			return false;
		}

		/// <summary>
		///		Called whenever a block of this type has been placed.
		/// </summary>
		/// <param name="world">The world the block was placed in</param>
		/// <param name="state">The blockstate used when placing the block</param>
		/// <param name="position">The position the block was placed at</param>
		/// <returns>The blockstate to use</returns>
		public virtual BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			//return state;

			if (state.VariantMapper.IsMultiPart)
			{
				BlockStateResource blockStateResource;

				if (Alex.Instance.Resources.TryGetBlockState(state.Name, out blockStateResource))
				{
					return MultiPartModelHelper.GetBlockState(world, position, state, blockStateResource);
				}
			}

			return state;
		}

		/// <summary>
		///		Called when a block adjacent to this block has been changed.
		/// </summary>
		/// <param name="world">The world the event occured in</param>
		/// <param name="position">The world position of the current block</param>
		/// <param name="updatedBlock">The world position of the updated block</param>
		public virtual void BlockUpdate(World world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			if (BlockState.VariantMapper.IsMultiPart)
			{
				BlockStateResource blockStateResource;

				if (Alex.Instance.Resources.TryGetBlockState(BlockState.Name, out blockStateResource))
				{
					var state = MultiPartModelHelper.GetBlockState(world, position, BlockState, blockStateResource);

					if (state.Id != BlockState.Id)
						world.SetBlockState(position, state);
				}
			}
		}

		public virtual bool PlaceBlock(World world,
			Player player,
			BlockCoordinates position,
			BlockFace face,
			Vector3 cursorPosition)
		{
			//BlockCoordinates target = position;
			var existingBlockState = world.GetBlockState(position);
			var existingBlock = existingBlockState.Block;

			if (!existingBlock.BlockMaterial.IsReplaceable)
			{
				position += face.GetBlockCoordinates();
			}

			world.SetBlockState(position, BlockState);

			return true;
		}

		/// <summary>
		///		Calculates the required tick time for this block to break.
		/// </summary>
		/// <param name="miningTool">The tool used to mine the block</param>
		/// <returns>The time required for the block to break</returns>
		public double GetBreakTime(Item miningTool)
		{
			ItemType toolItemType = ItemType.Hand;
			ItemMaterial toolItemMaterial = ItemMaterial.None;

			if (miningTool.Count > 0 && !(miningTool is ItemAir))
			{
				toolItemType = miningTool.ItemType;
				toolItemMaterial = miningTool.Material;
			}

			double secondsForBreak = BlockMaterial.Hardness;
			bool isHarvestable = true;

			if (BlockMaterial.IsToolRequired)
			{
				isHarvestable = BlockMaterial.CanUseTool(toolItemType, toolItemMaterial);
			}

			if (secondsForBreak <= 0)
			{
				secondsForBreak = 0.5f;
			}

			if (isHarvestable)
			{
				secondsForBreak *= 1.5;
			}
			else
			{
				secondsForBreak *= 5;
			}

			int tierMultiplier = 1;

			if (BlockMaterial.CanUseTool(toolItemType, toolItemMaterial))
			{
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
						else if (this is Minecraft.Leaves.Leaves || this is AcaciaLeaves || this is Cobweb)
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
					/*case ItemType.Shovel:
					case ItemType.Axe:
					case ItemType.PickAxe:
					case ItemType.Hoe:
						return secondsForBreak / tierMultiplier;*/
				}
			}

			return secondsForBreak / tierMultiplier;
		}

		/// <summary>
		///		Used to determine if a blockface should be culled or not
		/// </summary>
		/// <param name="face">The face</param>
		/// <param name="neighbor">The block adjacent to this block at specified face</param>
		/// <returns>True if the face should be rendered</returns>
		public virtual bool ShouldRenderFace(BlockFace face, Block neighbor)
		{
			if (!neighbor.Renderable)
				return true;

			if (Transparent)
			{
				if (Solid)
				{
					if (!FancyGraphics)
					{
						if (neighbor.Solid && neighbor.IsFullCube && neighbor.Transparent) //Was isfullblock
							return false;
					}

					//	if (IsFullCube && Name.Equals(block.Name)) return false;
					if (neighbor.Solid && (neighbor.Transparent || !neighbor.IsFullCube))
					{
						//var block = world.GetBlock(pos.X, pos.Y, pos.Z);
						if (!BlockMaterial.IsOpaque && !neighbor.BlockMaterial.IsOpaque)
							return false;

						//if (!IsFullBlock || !neighbor.IsFullBlock) return true;
					}

					//If neighbor is solid & not transparent. Hmmm?
					if (neighbor.Solid && !(neighbor.Transparent || !neighbor.IsFullCube)) return false;
				}
				else
				{
					//  if (neighbor.Solid && neighbor.Transparent && neighbor.IsFullCube)
					//     return true;

					if (neighbor.Solid && !(neighbor.Transparent || neighbor.IsFullCube)) return false;
				}
			}


			if (Solid && (neighbor.Transparent || !neighbor.IsFullCube)) return true;

			//   if (me.Transparent && block.Transparent && !block.Solid) return false;
			if (Transparent) return true;
			if (!Transparent && (neighbor.Transparent || !neighbor.IsFullCube)) return true;
			if (neighbor.Solid && !(neighbor.Transparent || !neighbor.IsFullCube)) return false;
			if (Solid && neighbor.Solid && neighbor.IsFullCube) return false;

			return true;
		}

		/// <summary>
		///		Determines whether or not this block can "attach" to an adjacent block.
		///		This is used for blocks like fences.
		/// </summary>
		/// <param name="face"></param>
		/// <param name="block">The adjacent block</param>
		/// <returns></returns>
		public virtual bool CanAttach(BlockFace face, Block block)
		{
			return block.Solid && (block.IsFullCube);
		}

		protected static string GetShape(BlockState state)
		{
			if (state.TryGetValue("shape", out string facingValue))
			{
				return facingValue;
			}

			return string.Empty;
		}

		protected static BlockFace GetFacing(BlockState state)
		{
			if (state.TryGetValue("facing", out string facingValue)
			    && Enum.TryParse<BlockFace>(facingValue, true, out BlockFace face))
			{
				return face;
			}

			return BlockFace.None;
		}

		public string DisplayName { get; set; } = null;

		public override string ToString()
		{
			return DisplayName ?? GetType().Name;
		}

		public ResourceLocation Location { get; private set; }

		public IRegistryEntry<Block> WithLocation(ResourceLocation location)
		{
			Location = location;

			return this;
		}

		public Block Value => this;

		public static readonly PropertyFace Facing = new PropertyFace("facing");

		public virtual bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
		{
			switch (prop)
			{
				case "facing":
					stateProperty = Facing;

					return true;

				case "waterlogged":
					stateProperty = WaterLogged;

					return true;
			}

			stateProperty = new PropertyString(prop);

			return true;
		}

		public virtual bool Interact(Player player, Item item, Vector3 cursorPosition)
		{
			return false;
		}
	}
}