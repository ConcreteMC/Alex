using Alex.Graphics.Models.Entity;

namespace Alex.Entities
{
	public class PlayerSkinFlags
	{
		public byte Value { get; set; } = 0;

		public bool CapeEnabled        => (Value & 0x01) != 0;
		public bool JacketEnabled      => (Value & 0x02) != 0;
		public bool LeftSleeveEnabled  => (Value & 0x04) != 0;
		public bool RightSleeveEnabled => (Value & 0x08) != 0;
		public bool LeftPantsEnabled   => (Value & 0x10) != 0;
		public bool RightPantsEnabled  => (Value & 0x20) != 0;
		public bool HatEnabled         => (Value & 0x40) != 0;
		
		public void ApplyTo(EntityModelRenderer renderer)
		{
			Set(renderer, "cape", CapeEnabled);
			Set(renderer, "jacket", JacketEnabled);
			Set(renderer, "leftSleeve", LeftSleeveEnabled);
			Set(renderer, "rightSleeve", RightSleeveEnabled);
			Set(renderer, "leftPants", LeftPantsEnabled);
			Set(renderer, "rightPants", RightPantsEnabled);
			Set(renderer, "hat", HatEnabled);
		}

		private void Set(EntityModelRenderer renderer, string bone, bool value)
		{
			if (renderer.GetBone(bone, out var boneValue))
			{
				boneValue.Rendered = value;
			}
		}
	}
}