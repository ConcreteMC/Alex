using Alex.API.Network;
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
		}
	}
}