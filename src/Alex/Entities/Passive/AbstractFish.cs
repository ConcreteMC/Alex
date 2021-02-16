using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class AbstractFish : WaterMob
	{
		/// <inheritdoc />
		protected AbstractFish(EntityType mobTypes, World level) : base(mobTypes, level)
		{
			
		}
	}
}