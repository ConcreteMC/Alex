using System;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Zombie : HostileMob
	{
		public Zombie(World level) : base( level)
		{
			Height = 1.95;
			Width = 0.6;
		}
	}
}
