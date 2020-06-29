using Alex.Net;
using Alex.Worlds;

namespace Alex.Entities.Throwable
{
	public class ArrowEntity : ThrowableEntity
	{
		/// <inheritdoc />
		public ArrowEntity(World level, NetworkProvider network) : base((int) EntityType.Arrow, level, network) { }
	}
}