using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Passive
{
	public abstract class PassiveMob : AgeableEntity
	{
		protected PassiveMob(World level) : base(level)
		{
			base.MapIcon.Color = Color.Green;
		}
	}
}