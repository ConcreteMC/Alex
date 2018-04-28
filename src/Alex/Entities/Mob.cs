using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	public class Mob : Entity
	{
		public Mob(int entityTypeId, World level, INetworkProvider network) : base(entityTypeId, level, network)
		{
			Width = Length = 0.6;
			Height = 1.80;
		}

		public Mob(EntityType mobTypes, World level, INetworkProvider network) : this((int)mobTypes, level, network)
		{
		}

		public override void OnTick()
		{
			base.OnTick();
		}
	}
}
