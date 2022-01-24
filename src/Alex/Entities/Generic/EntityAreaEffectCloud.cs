using Alex.Networking.Java.Packets.Play;
using Alex.Particles;
using Alex.Worlds;
using MiNET.Particles;
using MiNET.Utils.Metadata;
using NLog;
using MetadataFloat = Alex.Networking.Java.Packets.Play.MetadataFloat;

namespace Alex.Entities.Generic
{
	public class EntityAreaEffectCloud : Entity
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityAreaEffectCloud));

		private float _radius = 0.5f;

		public float Radius
		{
			get => _radius;
			set
			{
				_radius = value;

				Width = 2f * value;
				Height = 0.5f;
			}
		}

		public bool IgnoreRadiusAndShowPoint { get; set; } = false;

		public int Color { get; set; } = 0;
		public int? ParticleId { get; set; } = null;
		public int Duration { get; set; } = 5 * 20;
		public float RadiusPerTick { get; set; } = 0f;

		/// <inheritdoc />
		public EntityAreaEffectCloud(World level) : base(level)
		{
			NoAi = true;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 8 && entry is MetadataFloat radius)
			{
				Radius = radius.Value;
			}
			else if (entry.Index == 9 && entry is MetadataVarInt color)
			{
				Color = color.Value;
				//Radius = radius.Value;
			}
			else if (entry.Index == 10 && entry is MetadataBool ignoreRadiusAndShowAsPoint)
			{
				IgnoreRadiusAndShowPoint = ignoreRadiusAndShowAsPoint.Value;
				//Radius = radius.Value;
			}
			/*else if (entry.Index == 10 && entry is MetadataFloat radius)
			{
				Radius = radius.Value;
			}*/
		}

		/*
		 * public const AREA_EFFECT_CLOUD_DURATION = 95; //int
			public const AREA_EFFECT_CLOUD_SPAWN_TIME = 96; //int
			public const AREA_EFFECT_CLOUD_RADIUS_PER_TICK = 97; //float, usually negative
			public const AREA_EFFECT_CLOUD_RADIUS_CHANGE_ON_PICKUP = 98; //float
			public const AREA_EFFECT_CLOUD_PICKUP_COUNT = 99; //int
		 */

		/// <inheritdoc />
		protected override bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			if (flag == MiNET.Entities.Entity.MetadataFlags.PotionColor
			    && entry is MiNET.Utils.Metadata.MetadataInt potionColor)
			{
				Color = potionColor.Value;

				return true;
			}

			if ((int)flag == 61 && entry is MiNET.Utils.Metadata.MetadataFloat flt) //Cloud Radius
			{
				Radius = flt.Value;

				return true;
			}

			if ((int)flag == 62 && entry is MiNET.Utils.Metadata.MetadataInt cloudWaiting) //Cloud Waiting
			{
				//Radius = flt.Value;
				return true;
			}

			if ((int)flag == 63 && entry is MiNET.Utils.Metadata.MetadataInt particleId) //Cloud ParticleId
			{
				ParticleId = particleId.Value;

				return true;
			}

			if ((int)flag == 95 && entry is MiNET.Utils.Metadata.MetadataInt cloudDuration) //Cloud Duration
			{
				Age = 0;
				Duration = cloudDuration.Value;

				return true;
			}

			if ((int)flag == 97 && entry is MiNET.Utils.Metadata.MetadataFloat radiusPerTick) //Cloud Radius Per Tick
			{
				RadiusPerTick = radiusPerTick.Value;

				return true;
			}

			return base.HandleMetadata(flag, entry);
		}

		/// <inheritdoc />
		public override void OnTick()
		{
			base.OnTick();

			if (Age > Duration)
			{
				return;
				//Stop. Kill. Destroy.
			}

			if (ParticleId.HasValue)
			{
				if (Alex.Instance.ParticleManager.SpawnParticle((ParticleType)ParticleId, RenderLocation, Color)) { }
			}
		}
	}
}