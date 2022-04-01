using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using ConcreteMC.MolangSharp.Attributes;
using MiNET.Utils.Metadata;
using MetadataByte = Alex.Networking.Java.Packets.Play.MetadataByte;

namespace Alex.Entities
{
	public class Mob : Insentient
	{
		private int _variant = 0;

		public Mob(World level) : base(level)
		{
			Width = 0.6;
			Height = 1.80;
		}

		[MoProperty("death_ticks")] public int DeathTicks => HealthManager.IsDying ? HealthManager.DyingTime : 0;

		[MoProperty("variant")]
		public int Variant
		{
			get => _variant;
			set
			{
				var oldValue = _variant;
				_variant = value;
				AnimationController?.InvokeRenderControllerUpdate();
				VariantChanged(oldValue, value);
			}
		}

		public bool IsAggressive { get; set; } = false;

		protected virtual void VariantChanged(int oldVariant, int newVariant) { }

		/// <inheritdoc />
		protected override void OnModelUpdated()
		{
			base.OnModelUpdated();
		}

		/// <inheritdoc />
		protected override bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			if (flag == MiNET.Entities.Entity.MetadataFlags.Variant && entry is MetadataInt mti)
			{
				Variant = mti.Value;

				return true;
			}

			return base.HandleMetadata(flag, entry);
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 15 && entry is MetadataByte data)
			{
				NoAi = (data.Value & 0x01) != 0;
				IsLeftHanded = (data.Value & 0x02) != 0;
				IsAggressive = (data.Value & 0x04) != 0;
			}
		}

		public override void OnTick()
		{
			base.OnTick();
		}
	}
}