using Alex.MoLang.Attributes;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Enderman : HostileMob
	{
		[MoProperty("is_carrying_block")]
		public bool IsCarryingBlock { get; set; } = false;
		
		public Enderman(World level) : base(level)
		{
			Height = 2.9;
			Width = 0.6;
		}
	}
}
