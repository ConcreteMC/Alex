using Alex.Graphics.Models.Entity;

namespace Alex.Entities
{
	public class PlayerSkinFlags
	{
		public static PlayerSkinFlags Default => new PlayerSkinFlags()
		{
			Value = byte.MaxValue
		};
		
		public byte Value { get; set; } = 0;

		public bool CapeEnabled
		{
			get => Value.IsBitSet(0x01);
			set => Value = Value.SetBit(0x01, value);
		}

		public bool JacketEnabled
		{
			get => Value.IsBitSet(0x02);
			set => Value = Value.SetBit(0x02, value);
		}

		public bool LeftSleeveEnabled
		{
			get => Value.IsBitSet(0x04);
			set => Value = Value.SetBit(0x04, value);
		}

		public bool RightSleeveEnabled
		{
			get => Value.IsBitSet(0x08);
			set => Value = Value.SetBit(0x08, value);
		}

		public bool LeftPantsEnabled
		{
			get => Value.IsBitSet(0x10);
			set => Value = Value.SetBit(0x10, value);
		}

		public bool RightPantsEnabled
		{
			get => Value.IsBitSet(0x20);
			set => Value = Value.SetBit(0x20, value);
		}

		public bool HatEnabled
		{
			get => Value.IsBitSet(0x40);
			set => Value = Value.SetBit(0x40, value);
		}

		public void ApplyTo(ModelRenderer renderer)
		{
			Set(renderer, "cape", CapeEnabled);
			Set(renderer, "jacket", JacketEnabled);
			Set(renderer, "leftSleeve", LeftSleeveEnabled);
			Set(renderer, "rightSleeve", RightSleeveEnabled);
			Set(renderer, "leftPants", LeftPantsEnabled);
			Set(renderer, "rightPants", RightPantsEnabled);
			Set(renderer, "hat", HatEnabled);
		}

		private void Set(ModelRenderer renderer, string bone, bool value)
		{
			renderer.SetVisibility(bone, value);
		}
	}
}