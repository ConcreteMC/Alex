using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Creeper : HostileMob
	{
		private bool _isCharged = false;

		public bool IsCharged
		{
			get
			{
				return _isCharged;
			}
			set
			{
				_isCharged = value;

				TryUpdateGeometry(
					"minecraft:creeper", value ? "charged" : "default", value ? "charged" : "default");
			}
		}

		public Creeper(World level) : base((EntityType)33, level)
		{
			JavaEntityId = 50;
			Height = 1.7;
			Width = 0.6;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataBool charged)
			{
				IsCharged = charged.Value;
			}
			else if (entry.Index == 17 && entry is MetadataBool ignited)
			{
				IsIgnited = ignited.Value;
			}
		}
	}
}
