using System;
using System.Linq;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Properties;
using Alex.Common.Blocks;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework;

namespace Alex.Entities.BlockEntities
{
	public class BedBlockEntity : BlockEntity
	{
		private EntityDescription _entityDescription = null;
		private BlockFace _facing = BlockFace.North;

		/// <inheritdoc />
		public BedBlockEntity(World level) : base(level)
		{
			Width = 1f;
			Height = 1f;
			
			Offset = new Vector3(0.5f, 0f, 0.5f);
		}

		private float _yRotation = 0f;
		public BlockFace Facing
		{
			get => _facing;
			set
			{
				_facing = value;
				
				var rotation = Rotation;
				rotation.X = -90f;
				switch (value)
				{
					case BlockFace.East:
						rotation.Y = 90f;
						break;

					case BlockFace.West:
						rotation.Y = 270f;
						break;

					case BlockFace.North:
						rotation.Y = 180f;
						break;

					case BlockFace.South:
						rotation.Y = 0f;
						break;
				}

				Rotation = rotation;
			}
		}

		/// <inheritdoc />
		protected override void ReadFrom(NbtCompound compound)
		{
			base.ReadFrom(compound);
			
		}

		private static readonly PropertyFace FACING = new PropertyFace("facing");
		/// <inheritdoc />
		protected override bool BlockChanged(Block oldBlock, Block newBlock)
		{
			base.BlockChanged(oldBlock, newBlock);

			if (newBlock == null || !(newBlock is Bed bed))
				return false;

			if (_entityDescription == null)
			{
				if (Alex.Instance.Resources.TryGetEntityDefinition(
					"minecraft:bed", out var entityDescription, out var source))
				{
					_entityDescription = entityDescription;
					AnimationController?.UpdateEntityDefinition(source, entityDescription);
				}
			}
			
			Facing = FACING.GetValue(bed.BlockState);
			
			if (!Bed.PART.GetValue(bed.BlockState))
			{
				ModelRenderer = null;
				return true;
			}

			if (ModelRenderer == null)
			{
				if (ModelFactory.TryGetModel("geometry.bed", out var geo))
				{
					if (EntityModelRenderer.TryGetRenderer(geo, out var renderer))
					{
						ModelRenderer = renderer;
					}
				}

				if (_entityDescription != null)
				{
					string texture = "default";

					switch (bed.Variant)
					{
						case BedColor.LightBlue:
							texture = "light_blue";
							break;

						case BedColor.LightGray:
							texture = "default";
							break;
						case BedColor.Silver:
						case BedColor.Cyan:
						case BedColor.Purple:
						case BedColor.Blue:
						case BedColor.Brown:
						case BedColor.Green:
						case BedColor.Red:
						case BedColor.Black:
							texture = bed.Variant.ToString().ToLower();
							break;
						
					}
					if (_entityDescription.Textures.TryGetValue(texture, out var texturePath))
					{
						if (Alex.Instance.Resources.TryGetBedrockBitmap(texturePath, out var bmp))
						{
							Texture = TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, bmp);
						}
					}
				}
			}

			return true;
		}
	}
	
	public enum BedColor
	{
		White,
		Orange,
		Magenta,
		LightBlue,
		Yellow,
		Lime,
		Pink,
		Gray,
		LightGray,
		Silver,
		Cyan,
		Purple,
		Blue,
		Brown,
		Green,
		Red,
		Black
	}
}