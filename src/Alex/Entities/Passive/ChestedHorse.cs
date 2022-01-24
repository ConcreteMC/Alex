using Alex.MoLang.Attributes;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public abstract class ChestedHorse : AbstractHorse
	{
		/// <inheritdoc />
		[MoProperty("is_chested")]
		public override bool IsChested
		{
			get
			{
				return base.IsChested;
			}
			set
			{
				base.IsChested = value;

				InvokeControllerUpdate();
				/*	var modelRenderer = ModelRenderer;
	
					if (modelRenderer != null)
					{
						modelRenderer.SetVisibility("Bag1", value);
						modelRenderer.SetVisibility("Bag2", value);
					}*/
			}
		}

		/// <inheritdoc />
		protected ChestedHorse(World level) : base(level) { }

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 19 && entry is MetadataBool val)
			{
				IsChested = val.Value;
			}
		}
	}
}