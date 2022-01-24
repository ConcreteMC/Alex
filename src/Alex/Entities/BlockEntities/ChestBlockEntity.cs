using System;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Properties;
using Alex.Common.Blocks;
using Alex.Common.Resources;
using Alex.Common.Utils.Vectors;
using Alex.Graphics.Models;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.Graphics.Models.Entity.BlockEntities;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework;

namespace Alex.Entities.BlockEntities
{
	public class ChestBlockEntity : BlockEntity
	{
		private BoneMatrices HeadBone { get; set; }
		private BoneMatrices Body { get; set; }

		/// <inheritdoc />
		public ChestBlockEntity(World level) : base(level)
		{
			Type = new ResourceLocation("minecraft:chest");

			Width = 1f;
			Height = 1f;
			
			AnimationController.Enabled = true;
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
			if (ModelRenderer.GetBoneTransform("head", out var head))
			{
				HeadBone = head;
			}

			if (ModelRenderer.GetBoneTransform("body", out var body))
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

					HeadBone.RotateOverTime(targetRotation, 250);
				}
			}
		}

		private BlockFace _rotation = BlockFace.North;
		private float _yRotation = 0f;

		public BlockFace Facing
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

		private ChestType _chestType = ChestType.Single;

		public ChestType ChestType
		{
			get
			{
				return _chestType;
			}
			set
			{
				_chestType = value;

				if ((value & ChestType.Double) != 0)
				{
					if ((value & ChestType.LeftHalf) != 0)
					{
						if (new DoubleChestEntityModel().TryGetRenderer(out var renderer))
						{
							ModelRenderer = renderer;
							Texture = BlockEntityFactory.DoubleChestTexture;

							Offset = new Vector3(0f, 0f, 0.5f);
						}
					}
					else if ((value & ChestType.RightHalf) != 0)
					{
						IsInvisible = true;
						ModelRenderer = null;
						Texture = null;
					}
				}
				else if ((value & ChestType.Single) != 0)
				{
					if (new ChestEntityModel().TryGetRenderer(out var renderer))
					{
						ModelRenderer = renderer;
						Texture = BlockEntityFactory.ChestTexture;
						Body.Position = Vector3.Zero;
					}
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

		private static readonly PropertyFace FACING = new PropertyFace("facing");

		/// <inheritdoc />
		protected override bool BlockChanged(Block oldBlock, Block newBlock)
		{
			base.BlockChanged(oldBlock, newBlock);

			if (newBlock == null || !(newBlock is Chest))
				return false;

			if (newBlock.BlockState.TryGetValue("type", out string type))
			{
				BlockEntities.ChestType chestType = 0;

				switch (type)
				{
					case "single":
						chestType = ChestType.Single;

						break;

					case "left":
						chestType = ChestType.Double | ChestType.LeftHalf;

						break;

					case "right":
						chestType = ChestType.Double | ChestType.RightHalf;

						break;
				}

				ChestType = chestType;
			}


			Facing = FACING.GetValue(newBlock.BlockState);


			return true;
		}

		/// <inheritdoc />
		protected override void ReadFrom(NbtCompound compound)
		{
			base.ReadFrom(compound);

			if (compound.TryGet("pairx", out NbtInt pairX) && compound.TryGet("pairz", out NbtInt pairZ))
			{
				var pairPos = new BlockCoordinates(pairX.Value, 0, pairZ.Value);
				var myPos = new BlockCoordinates(X, 0, Z);
				var difference = myPos - pairPos;

				switch (Facing)
				{
					case BlockFace.South:
					case BlockFace.North:
						if (difference == BlockCoordinates.Forwards)
						{
							ChestType = ChestType.RightHalf | ChestType.Double;
						}
						else if (difference == BlockCoordinates.Backwards)
						{
							ChestType = ChestType.LeftHalf | ChestType.Double;
						}

						break;

					case BlockFace.West:
					case BlockFace.East:
						if (difference == BlockCoordinates.Left)
						{
							ChestType = ChestType.RightHalf | ChestType.Double;
						}
						else if (difference == BlockCoordinates.Right)
						{
							ChestType = ChestType.LeftHalf | ChestType.Double;
						}

						break;
				}
			}
		}
	}

	[Flags]
	public enum ChestType
	{
		Single = 0x01,
		Double = 0x02,
		LeftHalf = 0x04,
		RightHalf = 0x08
	}
}