using System;
using System.Collections.Generic;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.Graphics.Models.Entity.BlockEntities;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.BlockEntities
{
	public class EnderChestBlockEntity : ChestBlockEntity
	{
		/// <inheritdoc />
		public EnderChestBlockEntity(Block block, World level) : base(block, level)
		{
			if (EntityModelRenderer.TryGetModel(new ChestEntityModel(), out var renderer))
			{
				ModelRenderer = renderer;
			}
			
			Texture = BlockEntityFactory.EnderChestTexture;
		}
	}
}