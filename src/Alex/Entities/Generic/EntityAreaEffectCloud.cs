using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
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

		/// <inheritdoc />
		public EntityAreaEffectCloud(World level) : base(level)
		{
			NoAi = true;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);
			
			if (entry.Index == 7 && entry is MetadataFloat radius)
			{
				Radius = radius.Value;
			}
			else if (entry.Index == 8 && entry is MetadataVarInt color)
			{
				Color = color.Value;
				//Radius = radius.Value;
			}
			else if (entry.Index == 9 && entry is MetadataBool ignoreRadiusAndShowAsPoint)
			{
				IgnoreRadiusAndShowPoint = ignoreRadiusAndShowAsPoint.Value;
				//Radius = radius.Value;
			}
			/*else if (entry.Index == 10 && entry is MetadataFloat radius)
			{
				Radius = radius.Value;
			}*/
		}

		/// <inheritdoc />
		protected override bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			Log.Info($"EffectCloud! Flag: {flag} Entry: {entry}");
			
			return base.HandleMetadata(flag, entry);
		}

	}
}