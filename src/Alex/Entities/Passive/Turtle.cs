using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using ConcreteMC.MolangSharp.Attributes;

namespace Alex.Entities.Passive
{
	public class Turtle : PassiveMob
	{
		[MoProperty("is_laying_egg")] public bool IsLayingEgg { get; set; }
		[MoProperty("is_pregnant")] public bool IsPregnant { get; set; }

		/// <inheritdoc />
		public Turtle(World level) : base(level) { }

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 19 && entry is MetadataBool layingEgg)
			{
				IsLayingEgg = layingEgg.Value;
			}
		}
	}
}