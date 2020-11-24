using System;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
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
		private EntityModelRenderer.ModelBone HeadBone { get; set; }
		private EntityModelRenderer.ModelBone Body     { get; set; }
		
		/// <inheritdoc />
		public ChestBlockEntity(Block block, World level, PooledTexture2D texture) : base(level, block)
		{
			Width = 16;
			Height = 16;
			
			ModelRenderer = new EntityModelRenderer(new ChestEntityModel(), texture);
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
			}
		}

		private bool _open;
		public bool Open
		{
			get
			{
				return _open;
			}
			set
			{
				_open = value;

				if (HeadBone != null)
				{
					var targetRotation = value ? new Vector3(90, 0, 0) : Vector3.Zero;

					HeadBone.Animations.Enqueue(
						new ServerAnimation(
							HeadBone, new ModelParameters(HeadBone.Rotation),
							new ModelParameters(targetRotation), TimeSpan.FromMilliseconds(250), false));
				}
			}
		}

		private BlockFace _rotation = BlockFace.North;
		public BlockFace Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				_rotation = value;

				var bodyRotation = Body.Rotation;
				var headRotation = HeadBone.Rotation;
				switch (value)
				{
					case BlockFace.East:
						bodyRotation.Y = 90f;
						break;

					case BlockFace.West:
						bodyRotation.Y = 270f;
						break;

					case BlockFace.North:
						bodyRotation.Y = 180f;
						break;

					case BlockFace.South:
						bodyRotation.Y = 0f;
						break;
				}

				Body.Rotation = bodyRotation;
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
			}
		}

		/// <inheritdoc />
		public override void HandleBlockAction(byte actionId, int parameter)
		{
			if (actionId == 1)
			{
				var chestOpen = parameter > 0;

				if (_open != chestOpen)
				{
					Open = chestOpen;
				}
			}
		}

		/// <inheritdoc />
		protected override void BlockChanged(Block oldBlock, Block newBlock)
		{
			base.BlockChanged(oldBlock, newBlock);

			if (newBlock == null)
				return;

			if (newBlock.BlockState.TryGetValue("facing", out string value))
			{
				if (Enum.TryParse(typeof(BlockFace), value, true, out var val))
				{
					Rotation = (BlockFace) val;
				}
			}
		}
	}
}