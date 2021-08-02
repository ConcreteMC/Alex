using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Hostile
{
	public abstract class HostileMob : Mob
	{
		protected HostileMob(World level)
			: base(level)
		{
			base.MapIcon.Color = Color.Red;
		}
	}
}
