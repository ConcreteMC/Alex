using Alex.Common.Graphics;
using Alex.Graphics.Models.Items;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Worlds;
using Alex.Worlds.Multiplayer;
using Alex.Worlds.Multiplayer.Java;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	public class ItemBaseEntity : ThrowableEntity
	{
		/// <inheritdoc />
		public ItemBaseEntity(World level) : base(level)
		{
			
		}
		
		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);
			
			if (entry.Index == 7 && entry is MetadataSlot slot)
			{
				var item = JavaWorldProvider.GetItemFromSlotData(slot.Value);
				if (item != null)
				{
					SetItem(item);
				}
			}
		}
		
		protected new IItemRenderer ItemRenderer { get; set; } = null;
		protected bool CanRender { get; set; } = false;
		public virtual void SetItem(Item item)
		{
			if (item.Renderer != null)
			{
				CanRender = true;
				ItemRenderer = item.Renderer;
				ItemRenderer.DisplayPosition = DisplayPosition.Ground;

				//Scale = (float) (item is ItemBlock ? (1f / (1f / Width)) : (1f / 32f));
				//	ItemRenderer.ActiveDisplayItem = null;
			}
			else
			{
				CanRender = false;
			}
		}
		
		public override void Update(IUpdateArgs args)
		{
			if (CanRender)
			{
				ItemRenderer.Update(
					args);
			}
		}
		
		public override int Render(IRenderArgs renderArgs, bool useCulling)
		{
			if (!CanRender)
				return 0;

			var itemRenderer = ItemRenderer;

			if (itemRenderer == null)
				return 0;
			
			return itemRenderer.Render(renderArgs, null, Matrix.Identity * Matrix.CreateScale(Scale)
			                                                             * Matrix.CreateRotationY(MathHelper.ToRadians(KnownPosition.Yaw))
			                                                             * Matrix.CreateTranslation(KnownPosition.ToVector3()));
		}
	}
}