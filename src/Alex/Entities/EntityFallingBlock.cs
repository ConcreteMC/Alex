using Alex.API.Network;
using Alex.Worlds;
using MiNET.Utils;

namespace Alex.Entities
{
	public class EntityFallingBlock : ItemEntity
	{
		/// <inheritdoc />
		public EntityFallingBlock(World level, INetworkProvider network) : base(level, network) { }
	}
}