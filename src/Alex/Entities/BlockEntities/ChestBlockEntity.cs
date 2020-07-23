using System;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.BlockEntities
{
	public class ChestBlockEntity : BlockEntity
	{
		private EntityModelRenderer.ModelBone HeadBone { get; }
		/// <inheritdoc />
		public ChestBlockEntity(Block block, World level, PooledTexture2D texture) : base(level, block)
		{
			Width = 16;
			Height = 16;
			
			ModelRenderer = new EntityModelRenderer(new ChestModel(), texture);

			if (ModelRenderer.GetBone("head", out var head))
			{
				HeadBone = head;
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
							HeadBone, new ModelParameters(HeadBone.Rotation, Vector3.Zero),
							new ModelParameters(targetRotation, Vector3.Zero), TimeSpan.FromMilliseconds(250), false));
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

				switch (value)
				{
					case BlockFace.East:
						KnownPosition.Yaw = 90f;
						break;

					case BlockFace.West:
						KnownPosition.Yaw = 270f;
						break;

					case BlockFace.North:
						KnownPosition.Yaw = 0;
						break;

					case BlockFace.South:
						KnownPosition.Yaw = 180f;
						break;
				}
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
		public override void HandleBlockAction(byte actionId, byte parameter)
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

		/// <inheritdoc />
		public override PlayerLocation KnownPosition
		{
			get
			{
				return base.KnownPosition + new Vector3(0.5f, 0, 0.5f);
			}
			set
			{
				base.KnownPosition = value;
			}
		}

		private class ChestModel : EntityModel
		{
			public ChestModel()
			{
				Description = new ModelDescription()
				{
					Identifier = "geometry.alex.chest",
					TextureHeight = 64,
					TextureWidth = 64
				};
				Bones = new EntityModelBone[]
				{
					new EntityModelBone()
					{
						Name = "body",
						Pivot = Vector3.Zero,
						Cubes = new []
						{
							new EntityModelCube()
							{
								Origin = new Vector3(-7, -10, -7),
								Size = new Vector3(14, 10, 14),
								Pivot = Vector3.Zero,
								Rotation = new Vector3(180, 0,0),
								Uv = new Vector2(0, 19)
							}
						}
					},
					new EntityModelBone()
					{
						Name = "head",
						Parent = "body",
						Pivot = new Vector3(0, 10, 7),
						Cubes = new []
						{
							new EntityModelCube()
							{
								Origin = new Vector3(-7, -15, -7),
								Size = new Vector3(14, 5, 14),
								Pivot = Vector3.Zero,
								Rotation = new Vector3(180, 0,0),
								Uv = new Vector2(0, 0)
							},
							new EntityModelCube()
							{
								Origin = new Vector3(-1, 8, -8),
								Size = new Vector3(2, 4, 1),
								Uv = Vector2.Zero
							}
						}
					}
				};
			}
		}
	}
}