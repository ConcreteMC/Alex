using System.ComponentModel;
using System.Runtime.Serialization;
using Alex.MoLang.Attributes;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class Cat : TameableMob
	{
		private CatType _variant = CatType.Black;

		public CatType Variant
		{
			get
			{
				return _variant;
			}
			set
			{
				_variant = value;

				string stringType = value.ToString().ToLower();

				if (value == CatType.BritishShorthair)
				{
					stringType = "british";
				}
				else if (value == CatType.AllBlack)
				{
					stringType = "all_black";
				}

				if (IsTamed)
				{
					stringType = $"{stringType}_tame";
				}

				//TryUpdateTexture("minecraft:cat", stringType);
			}
		}

		/// <inheritdoc />
		public Cat(World level) : base(EntityType.Cat, level)
		{
			Height = 0.7;
			Width = 0.6;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 19 && entry is MetadataVarInt varInt)
			{
				Variant = (CatType)varInt.Value;
			}
		}

		/// <inheritdoc />
		public override void EntityHurt()
		{
			base.EntityHurt();
			Alex.Instance.AudioEngine.PlaySound("mob.cat.hit", RenderLocation, 1f, 1f);
		}

		[MoProperty("variant")] public int QueryVariant => (int)Variant;

		public enum CatType
		{
			Tabby = 0,
			Black = 1,
			Red = 2,
			Siamese = 3,
			BritishShorthair = 4,
			Calico = 5,
			Persian = 6,
			Ragdoll = 7,
			White = 8,
			AllBlack = 9
		}
	}
}