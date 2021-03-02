using Alex.API.Network;
using Alex.Net;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities
{
	public class Mob : Insentient
	{
		public Mob(World level) : base(level)
		{
			Width = 0.6;
			Height = 1.80;
		}

		public override void OnTick()
		{
			base.OnTick();
		}
	}
}
