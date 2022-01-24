using System;
using System.Collections.Generic;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.Graphics.Models.Entity.BlockEntities;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.BlockEntities
{
	public class EnderChestBlockEntity : ChestBlockEntity
	{
		/// <inheritdoc />
		public EnderChestBlockEntity(World level) : base(level)
		{
			if (new ChestEntityModel().TryGetRenderer(out var renderer))
			{
				ModelRenderer = renderer;
			}

			Texture = BlockEntityFactory.EnderChestTexture;
		}
	}
}