using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class AbstractFish : WaterMob
	{
		/// <inheritdoc />
		protected AbstractFish(World level) : base(level) { }
	}
}