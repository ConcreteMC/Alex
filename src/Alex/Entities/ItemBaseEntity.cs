using Alex.API.Graphics;
using Alex.Graphics.Models.Items;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Java;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	public class ItemBaseEntity : ThrowableEntity
	{
		/// <inheritdoc />
		public ItemBaseEntity(World level) : base(level, null)
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
				ItemRenderer = item.Renderer.Clone();
				ItemRenderer.DisplayPosition = DisplayPosition.Ground;
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
				ItemRenderer.Update(args, Matrix.Identity * Matrix.CreateScale(Scale)
				                                           * Matrix.CreateRotationY(MathHelper.ToRadians(KnownPosition.Yaw))
				                                           * Matrix.CreateTranslation(KnownPosition.ToVector3()), Color.White.ToVector3(), KnownPosition);
				
				//ItemRenderer?.Update(
				//	Matrix.Identity * Matrix.CreateScale(Scale)
				//	                * Matrix.CreateRotationY(MathHelper.ToRadians(KnownPosition.Yaw))
				//	                * Matrix.CreateTranslation(KnownPosition.ToVector3()), KnownPosition);

				ItemRenderer?.Update(args.GraphicsDevice, args.Camera);
			}
		}
		
		public override void Render(IRenderArgs renderArgs)
		{
			if (!CanRender)
				return;
            
			ItemRenderer?.Render(renderArgs, false, out _);
		}

		/// <inheritdoc />
		public override void OnTick()
		{
			base.OnTick();

			var itemRenderer = ItemRenderer;

			if (itemRenderer != null)
			{
				itemRenderer.DiffuseColor =
					(new Color(245, 245, 225) *  ((1f / 16f) * SurroundingLightValue))
					* Level.BrightnessModifier;
			}
		}
	}
}