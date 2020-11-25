using System;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Entities.Models;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.BlockEntities;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework;

namespace Alex.Entities.BlockEntities
{
	public class SkullBlockEntity : BlockEntity
	{
		private static PooledTexture2D _skeleton = null,
			_witherSkeleton                      = null,
			_zombie                              = null,
			_creeper                             = null,
			_dragon                              = null;
		
		private EntityModelRenderer.ModelBone HeadBone { get; set; }
		
		/// <inheritdoc />
		public SkullBlockEntity(World level, Block block, PooledTexture2D texture) : base(level, block)
		{
			ModelRenderer = new EntityModelRenderer(new SkullBlockEntityModel(), texture);
			
			Width = 8;
			Height = 8;
		}

		protected override void UpdateModelParts()
		{
			//base.UpdateModelParts();
			if (ModelRenderer.GetBone("head", out var head))
			{
				HeadBone = head;
			}
		}
		
		private byte _rotation = 0;
		public byte Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				_rotation = Math.Clamp(value, (byte)0, (byte)15);
				
				var headRotation = HeadBone.Rotation;
				headRotation.Y          = _rotation * 22.5f;

				HeadBone.Rotation = headRotation;
				//HeadBone.Rotation = headRotation;
			}
		}

		/// <inheritdoc />
		protected override void BlockChanged(Block oldBlock, Block newBlock)
		{
			if (newBlock is Skull s)
			{
				switch (s.SkullType)
				{
					case SkullType.Player:
						break;

					case SkullType.Skeleton:
						if (_skeleton == null)
						{
							if (Alex.Instance.Resources.ResourcePack.TryGetBitmap(
								"minecraft:entity/skeleton/skeleton", out var bmp))
							{
								_skeleton = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, bmp);
							}
						}
						
						if (_skeleton != null)
						{
							ModelRenderer = new EntityModelRenderer(new SkullBlockEntityModel(), _skeleton);
							//ModelRenderer.Texture = _skeleton;
						}
						break;

					case SkullType.WitherSkeleton:
						if (_witherSkeleton == null)
						{
							if (Alex.Instance.Resources.ResourcePack.TryGetBitmap(
								"minecraft:entity/skeleton/wither_skeleton", out var bmp))
							{
								_witherSkeleton = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, bmp);
							}
						}
						
						if (_witherSkeleton != null)
						{
							ModelRenderer = new EntityModelRenderer(new SkullBlockEntityModel(), _witherSkeleton);
						}
						break;

					case SkullType.Zombie:
						if (_zombie == null)
						{
							if (Alex.Instance.Resources.ResourcePack.TryGetBitmap(
								"minecraft:entity/zombie/zombie", out var bmp))
							{
								_zombie = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, bmp);
							}
						}
						
						if (_zombie != null)
						{
							ModelRenderer = new EntityModelRenderer(new SkullBlockEntityModel(), _zombie);
						}
						break;

					case SkullType.Creeper:
						if (_creeper == null)
						{
							if (Alex.Instance.Resources.ResourcePack.TryGetBitmap(
								"minecraft:entity/creeper/creeper", out var bmp))
							{
								_creeper = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, bmp);
							}
						}
						
						if (_creeper != null)
						{
							ModelRenderer = new EntityModelRenderer(new SkullBlockEntityModel(), _creeper);
						}
						break;

					case SkullType.Dragon:
						if (_dragon == null)
						{
							if (Alex.Instance.Resources.ResourcePack.TryGetBitmap(
								"minecraft:entity/enderdragon/dragon", out var bmp))
							{
								_dragon = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, bmp);
							}
						}
						
						if (_dragon != null)
						{
							ModelRenderer = new EntityModelRenderer(new DragonHeadModel(), _dragon);
							//ModelRenderer.Texture = _dragon;
						}
						break;
				}
			}
			
			if (newBlock is WallSkull)
			{
				if (newBlock.BlockState.TryGetValue("facing", out var facing))
				{
					if (Enum.TryParse<BlockFace>(facing, true, out var face))
					{
						Offset = (face.Opposite().GetVector3() * 0.5f);
					}
				}
			}
			else if (newBlock is Skull)
			{
				if (newBlock.BlockState.TryGetValue("rotation", out var r))
				{
					if (byte.TryParse(r, out var rot))
					{
						Rotation = rot;
					}
				}
			}
		}

		/// <inheritdoc />
		public override void SetData(byte action, NbtCompound compound)
		{
			if (action == 4)
			{
				//var tag = compound.
			}
		}

		/// <inheritdoc />
		public override void HandleBlockAction(byte actionId, int parameter)
		{
			base.HandleBlockAction(actionId, parameter);
		}

		/// <inheritdoc />
		protected override void ReadFrom(NbtCompound compound)
		{
			
		}

		private Vector3 Offset { get; set; } = Vector3.Zero;// = new Vector3(0.5f, 0, 0.5f);
		/// <inheritdoc />
		public override PlayerLocation KnownPosition
		{
			get => base.KnownPosition + Offset;
			set => base.KnownPosition = value;
		}
	}
}