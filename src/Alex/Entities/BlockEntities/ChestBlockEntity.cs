using System;
using Alex.Blocks.Minecraft;
using Alex.Common.Blocks;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.Graphics.Models.Entity.BlockEntities;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.BlockEntities
{
	public class ChestBlockEntity : BlockEntity
	{
		private ModelBone HeadBone { get; set; }
		private ModelBone Body     { get; set; }
		
		/// <inheritdoc />
		public ChestBlockEntity(Block block, World level) : base(level, block)
		{
			Width = 1f;
			Height = 1f;
		}

		/// <inheritdoc />
		public override BoundingBox GetBoundingBox(Vector3 pos)
		{
			return new BoundingBox(pos, pos + Vector3.One);
		}
		
		/// <inheritdoc />
		public override BoundingBox GetVisibilityBoundingBox(Vector3 pos)
		{
			return new BoundingBox(pos, pos + new Vector3(1f, 1f, 1f));
		}

		/// <inheritdoc />
		protected override void UpdateModelParts()
		{
			//base.UpdateModelParts();
			if (ModelRenderer.GetBone("head", out var head))
			{
				HeadBone = head;
			}

			if (ModelRenderer.GetBone("body", out var body))
			{
				Body = body;
				
				var bodyRotation = Body.Rotation;
				bodyRotation.Y = _yRotation;
				Body.Rotation = bodyRotation;
			}
		}

		private int _viewers;
		public int Viewers
		{
			get
			{
				return _viewers;
			}
			set
			{
				_viewers = value;

				if (HeadBone != null)
				{
					var targetRotation = _viewers > 0 ? new Vector3(90, 0, 0) : Vector3.Zero;

					HeadBone.MoveOverTime(Vector3.Zero, targetRotation, TimeSpan.FromMilliseconds(250));
				}
			}
		}

		private BlockFace _rotation  = BlockFace.North;
		private float     _yRotation = 0f;
		public BlockFace Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				_rotation = value;
				
				//var headRotation = HeadBone.Rotation;
				switch (value)
				{
					case BlockFace.East:
						_yRotation = 90f;
						break;

					case BlockFace.West:
						_yRotation = 270f;
						break;

					case BlockFace.North:
						_yRotation = 180f;
						break;

					case BlockFace.South:
						_yRotation = 0f;
						break;
				}

				if (Body != null)
				{
					var bodyRotation = Body.Rotation;
					bodyRotation.Y = _yRotation;
					Body.Rotation = bodyRotation;
				}

				//HeadBone.Rotation = headRotation;
			}
		}

		private bool _isDoubleChest = false;
		public bool IsDoubleChest
		{
			get
			{
				return _isDoubleChest;
			}
			set
			{
				_isDoubleChest = value;

				if (value)
				{
					if (EntityModelRenderer.TryGetRenderer(new DoubleChestEntityModel(), out var renderer))
					{
						ModelRenderer = renderer;
					}
					
					Texture = BlockEntityFactory.DoubleChestTexture;
				}
				else
				{
					if (EntityModelRenderer.TryGetRenderer(new ChestEntityModel(), out var renderer))
					{
						ModelRenderer = renderer;
					}
					
					Texture = BlockEntityFactory.ChestTexture;
				}
			}
		}

		/// <inheritdoc />
		public override void HandleBlockAction(byte actionId, int parameter)
		{
			if (actionId == 1)
			{
				Viewers = parameter;
			}
		}

		/// <inheritdoc />
		protected override void BlockChanged(Block oldBlock, Block newBlock)
		{
			base.BlockChanged(oldBlock, newBlock);

			if (newBlock == null || !(newBlock is Chest))
				return;

			if (newBlock.BlockState.TryGetValue("type", out string type))
			{
				switch (type)
				{
					case "single":
						IsDoubleChest = false;
						break;
					case "left":
						IsDoubleChest = true;
						break;
					case "right":
						IsDoubleChest = true;
						break;
				}
			}
			
			if (newBlock.BlockState.TryGetValue("facing", out string value))
			{
				if (Enum.TryParse<BlockFace>(value, true, out var val))
				{
					Rotation = (BlockFace) val;
				}
			}
		}
	}
}