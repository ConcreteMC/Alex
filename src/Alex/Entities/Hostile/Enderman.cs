using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using ConcreteMC.MolangSharp.Attributes;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Enderman : HostileMob
	{
		[MoProperty("is_carrying_block")] public bool IsCarryingBlock { get; set; } = false;

		public Enderman(World level) : base(level)
		{
			Height = 2.9;
			Width = 0.6;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataOptBlockId optBlockId)
			{
				IsCarryingBlock = optBlockId.Value != 0;
			}
		}
	}
}