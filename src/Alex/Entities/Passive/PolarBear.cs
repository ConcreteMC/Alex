using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using ConcreteMC.MolangSharp.Attributes;

namespace Alex.Entities.Passive
{
	public class PolarBear : PassiveMob
	{
		[MoProperty("standing_scale")] public double StandingScale { get; set; } = 1d;

		public bool IsStandingUp { get; set; } = false;

		public PolarBear(World level) : base(level)
		{
			Height = 1.4;
			Width = 1.3;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 17 && entry is MetadataBool mdb)
			{
				IsStandingUp = mdb.Value;
			}
		}
	}
}