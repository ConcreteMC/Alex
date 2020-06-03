using Alex.API.Network;
using Alex.Items;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Worlds;
using MiNET.Utils;

namespace Alex.Entities
{
	public class EntityFallingBlock : ItemEntity
	{
		/// <inheritdoc />
		public EntityFallingBlock(World level, INetworkProvider network) : base(level, network)
		{
			Width = 1;
			Height = 1;
			Length = 1;

			DoRotation = false;
		}

		/// <inheritdoc />
		public override void SetItem(Item item)
		{
			base.SetItem(item);

			if (ItemRenderer != null)
			{
				ItemRenderer.DisplayPosition = DisplayPosition.Undefined;
			}
		}
	}
}