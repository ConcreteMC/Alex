using System;
using Alex.Blocks.Minecraft;
using Alex.Common.Blocks;
using Alex.Common.Graphics;
using Alex.Graphics.Models.Items;
using Alex.Items;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Entities.BlockEntities
{
	public class ItemFrameBlockEntity : BlockEntity
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ItemFrameBlockEntity));
		
		/// <inheritdoc />
		public ItemFrameBlockEntity(World level) : base(level)
		{
			Scale = 1f / 16f;
			Width = 0.98f;
			Height = 0.98f;
		}

		private float _yaw = 0f;
		private BlockFace _facing = BlockFace.North;

		/// <inheritdoc />
		protected override void ReadFrom(NbtCompound compound)
		{
			base.ReadFrom(compound);
			
			if (compound.TryGet("Item", out var itemTag))
			{
				var count = itemTag["Count"].ByteValue;
				var id = itemTag["id"].ShortValue;
				var damage = itemTag["Damage"].ShortValue;

				if (ItemFactory.TryGetItem(id, damage, out var item))
				{
					item.Count = count;
					SetItem(item);

					return;
				}
				
				if (ItemFactory.TryGetItem("minecraft:stone", out item))
				{
					SetItem(item);
				}
			}
		}

		protected new IItemRenderer ItemRenderer { get; set; } = null;

		private BlockFace Facing
		{
			get => _facing;
			set
			{
				switch (value)
				{
					case BlockFace.East:
						_yaw = 90f;
						break;

					case BlockFace.West:
						_yaw = 270f;
						break;

					case BlockFace.North:
						_yaw = 180f;
						break;

					case BlockFace.South:
						_yaw = 0f;
						break;
					
					default:
						return;
				}
				
				_facing = value;
				UpdateOffset();
			}
		}

		/// <inheritdoc />
		protected override bool BlockChanged(Block oldBlock, Block newBlock)
		{
			if (!(newBlock is ItemFrame itemFrame))
				return false;

			Facing = Block.Facing.GetValue(itemFrame.BlockState);
			//Offset = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathHelper.ToRadians(_yaw)));

			return base.BlockChanged(oldBlock, newBlock);
		}

		protected bool CanRender { get; set; } = false;
		public bool SetItem(Item item)
		{
			if (item.Renderer != null)
			{
				CanRender = true;
				ItemRenderer = item.Renderer;
				ItemRenderer.DisplayPosition = DisplayPosition.Fixed;

				UpdateOffset();

				return true;
			}
			else
			{
				CanRender = false;
			}

			return false;
		}

		private void UpdateOffset()
		{
			var itemRenderer = ItemRenderer;

			if (itemRenderer?.Model == null)
				return;
			
			if (itemRenderer.Model.Type == ModelType.Block)
			{
				Scale = 0.25f;
				Offset = Vector3.Transform(new Vector3(0.375f, 0.375f,-0.125f), Matrix.CreateRotationY(MathHelper.ToRadians(_yaw)));
			}
			else
			{
				Scale = 1f / 16f;
				Offset = Vector3.Transform(new Vector3(0f, 0f, 0.025f), Matrix.CreateRotationY(MathHelper.ToRadians(_yaw)));
			}
		}
		
		public override void Update(IUpdateArgs args)
		{
			if (CanRender)
			{
				ItemRenderer.Update(
					args, new Vector3(Scale));
			}
		}
		
		public override int Render(IRenderArgs renderArgs, bool useCulling)
		{
			if (!CanRender)
				return 0;

			var itemRenderer = ItemRenderer;

			if (itemRenderer == null)
				return 0;
			
			Matrix worldMatrix;
			var offset = new Vector3((float) Width / 2f, 0f, (float) Width / 2f);
			var knownPos = KnownPosition.ToVector3();

			worldMatrix = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(-offset)
			                                        * Matrix.CreateRotationY(MathHelper.ToRadians(_yaw))
			                                        * Matrix.CreateTranslation(offset)
			                                        * Matrix.CreateTranslation(knownPos);

				return itemRenderer.Render(renderArgs, null, worldMatrix);
		}
	}
}