using System.Drawing;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Entities.Passive
{
	public class Squid : PassiveMob
	{
		public Squid(World level) : base((EntityType)17, level)
		{
			JavaEntityId = 94;
			Height = 0.8;
			Width = 0.8;
		}
	}
}
