using System;
using Alex.Blocks.Minecraft;
using Alex.Common.Blocks;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.BlockEntities;
using Alex.Networking.Java.Packets.Play;
using Alex.Utils;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModelBone = Alex.Graphics.Models.ModelBone;

namespace Alex.Entities.BlockEntities
{
	public class SkullBlockEntity : BlockEntity
	{
		private static Texture2D _skeleton = null,
			_witherSkeleton                      = null,
			_zombie                              = null,
			_creeper                             = null,
			_dragon                              = null;
		
		private ModelBone HeadBone { get; set; }
		
		/// <inheritdoc />
		public SkullBlockEntity(World level, Texture2D texture) : base(level)
		{
			if (new SkullBlockEntityModel().TryGetRenderer(out var renderer))
			{
				ModelRenderer = renderer;
			}

			//ModelRenderer = new EntityModelRenderer(new SkullBlockEntityModel());
			Texture = texture;
			
			Width = 0.5f;
			Height =  0.5f;
		}
		
		/// <inheritdoc />
		public override BoundingBox GetVisibilityBoundingBox(Vector3 pos)
		{
			return new BoundingBox(pos, pos + new Vector3(1f, 1f, 1f));
		}

		protected override void UpdateModelParts()
		{
			//base.UpdateModelParts();
			if (ModelRenderer.GetBone("head", out var head))
			{
				var rot = head.Rotation;
				rot.Y = _yRotation;
				head.Rotation = rot;
				
				HeadBone = head;
			}
		}
		
		private byte  _rotation  = 0;
		private float _yRotation = 0f;
		public byte Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				_rotation = Math.Clamp(value, (byte)0, (byte)15);
				
				//var headRotation = HeadBone.Rotation;
				_yRotation          = _rotation * 22.5f;

				if (HeadBone != null)
				{
					var headRotation = HeadBone.Rotation;
					headRotation.Y = _yRotation;
					HeadBone.Rotation = headRotation;
				}

				//HeadBone.Rotation = headRotation;
			}
		}

		/// <inheritdoc />
		protected override bool BlockChanged(Block oldBlock, Block newBlock)
		{
			if (!(newBlock is Skull s))
				return false;
			
			{
				switch (s.SkullType)
				{
					case SkullType.Player:
						break;

					case SkullType.Skeleton:
						if (_skeleton == null)
						{
							if (Alex.Instance.Resources.TryGetBitmap(
								"minecraft:entity/skeleton/skeleton", out var bmp))
							{
								_skeleton = TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, bmp);
							}
						}
						
						if (_skeleton != null)
						{
							//ModelRenderer = new EntityModelRenderer(new SkullBlockEntityModel());
							Texture = _skeleton;
							//ModelRenderer.Texture = _skeleton;
						}
						break;

					case SkullType.WitherSkeleton:
						if (_witherSkeleton == null)
						{
							if (Alex.Instance.Resources.TryGetBitmap(
								"minecraft:entity/skeleton/wither_skeleton", out var bmp))
							{
								_witherSkeleton = TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, bmp);
							}
						}
						
						if (_witherSkeleton != null)
						{
							//ModelRenderer = new EntityModelRenderer(new SkullBlockEntityModel());
							Texture = _witherSkeleton;
						}
						break;

					case SkullType.Zombie:
						if (_zombie == null)
						{
							if (Alex.Instance.Resources.TryGetBitmap(
								"minecraft:entity/zombie/zombie", out var bmp))
							{
								_zombie = TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, bmp);
							}
						}
						
						if (_zombie != null)
						{
							//ModelRenderer = new EntityModelRenderer(new SkullBlockEntityModel());
							Texture = _zombie;
						}
						break;

					case SkullType.Creeper:
						if (_creeper == null)
						{
							if (Alex.Instance.Resources.TryGetBitmap(
								"minecraft:entity/creeper/creeper", out var bmp))
							{
								_creeper = TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, bmp);
							}
						}
						
						if (_creeper != null)
						{
							Texture = _creeper;
						}
						break;

					case SkullType.Dragon:
						if (_dragon == null)
						{
							if (Alex.Instance.Resources.TryGetBitmap(
								"minecraft:entity/enderdragon/dragon", out var bmp))
							{
								_dragon = TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, bmp);
							}
						}
						
						if (_dragon != null)
						{
							if (ModelFactory.TryGetModel("geometry.dragon_head", out var dragonHead))
							{
								if (dragonHead.TryGetRenderer(out var renderer))
								{
									ModelRenderer = renderer;
								}
								
								Texture = _dragon;
							}

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
			else
			{
				if (newBlock.BlockState.TryGetValue("rotation", out var r))
				{
					if (byte.TryParse(r, out var rot))
					{
						Rotation = rot;
					}
				}
			}
			
			return true;
		}

		/// <inheritdoc />
		public override void SetData(BlockEntityActionType action, NbtCompound compound)
		{
			if (action == BlockEntityActionType.SetRotationAndSkinOfModHead || action == BlockEntityActionType._Init)
			{
				ReadFrom(compound);
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
			if (compound == null)
				return;

			if (compound.TryGet<NbtByte>("Rot", out var rotation) || compound.TryGet<NbtByte>("rot", out rotation))
			{
				Rotation = rotation.Value;
			}
		}

		private Vector3 Offset { get; set; } = Vector3.Zero;// = new Vector3(0.5f, 0, 0.5f);
		/// <inheritdoc />
		public override PlayerLocation KnownPosition
		{
			get => base.KnownPosition;
			set => base.KnownPosition = value;
		}
		
		/// <inheritdoc />
		internal override PlayerLocation RenderLocation
		{
			get => base.RenderLocation + Offset;
			set
			{
				base.RenderLocation = value;
			}
		}
	}
}