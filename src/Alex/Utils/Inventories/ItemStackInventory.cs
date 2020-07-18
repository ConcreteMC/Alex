using Alex.Worlds.Multiplayer.Bedrock;
using MiNET.Utils;

namespace Alex.Utils.Inventories
{
	public class ItemStackInventory : BedrockInventory
	{
		private BedrockClient Client { get; }
		/// <inheritdoc />
		public ItemStackInventory(BedrockClient bedrockClient, int slots) : base(slots)
		{
			Client = bedrockClient;
		}

		public void HandleResponses(ItemStackResponses responses)
		{
			
		}
	}
}