using Alex.Graphics.Models.Entity;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public abstract class ChestedHorse : AbstractHorse
	{
		/// <inheritdoc />
		public override bool IsChested
		{
			get
			{
				return base.IsChested;
			}
			set
			{
				base.IsChested = value;

				var modelRenderer = ModelRenderer;

				if (modelRenderer != null && modelRenderer.Valid)
				{
					if (modelRenderer.GetBone("Bag1", out var bag1))
					{
						ToggleCubes(bag1, !IsChested);
					}
					
					if (modelRenderer.GetBone("Bag2", out var bag2))
					{
						ToggleCubes(bag2, !IsChested);
					}
				}
			}
		}

		/// <inheritdoc />
		protected ChestedHorse(EntityType type, World level) : base(type, level) { }

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 18 && entry is MetadataBool val)
			{
				IsChested = val.Value;
			}
		}
	}
}