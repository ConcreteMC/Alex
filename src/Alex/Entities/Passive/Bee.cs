using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Bee : PassiveMob
	{
		/// <inheritdoc />
		public Bee(World level) : base(level)
		{
			Width = 0.7;
			Height = 0.6;
		}
		
		public bool HasStung { get; set; }
		public bool HasNectar { get; set; }
		public int AngerTime { get; set; }

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 17 && entry is MetadataByte meta)
			{
				IsAngry = (meta.Value & 0x02) != 0;
				HasStung = (meta.Value & 0x04) != 0;
				HasNectar = (meta.Value & 0x08) != 0;
			}

			if (entry.Index == 18 && entry is MetadataVarInt angerTime)
			{
				AngerTime = angerTime.Value;
			}
		}
		
		/// <inheritdoc />
		public override void EntityDied()
		{
			base.EntityDied();
			Alex.Instance.AudioEngine.PlaySound("mob.bee.death", RenderLocation, 1f, 1f);
		}

		/// <inheritdoc />
		public override void EntityHurt()
		{
			base.EntityHurt();
			Alex.Instance.AudioEngine.PlaySound("mob.bee.hurt", RenderLocation, 1f, 1f);
		}
	}
}