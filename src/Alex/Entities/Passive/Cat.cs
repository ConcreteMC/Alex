using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using ConcreteMC.MolangSharp.Attributes;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class Cat : TameableMob
	{
		public CatType CatVariant
		{
			get
			{
				return (CatType)base.Variant;
			}
			set
			{
				base.Variant = (int) value;
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
				CatVariant = (CatType)varInt.Value;
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