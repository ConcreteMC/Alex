using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using ConcreteMC.MolangSharp.Attributes;

namespace Alex.Entities.Passive
{
	public class Pig : PassiveMob
	{
		private bool _hasSaddle;

		[MoProperty("is_saddled")]
		public bool HasSaddle
		{
			get => _hasSaddle;
			set
			{
				_hasSaddle = value;

				InvokeControllerUpdate();
				//	var modelRenderer = ModelRenderer;

				//if (modelRenderer != null)
				//{
				//	ModelRenderer.SetVisibility("Bag1", !IsChested);
				//}
			}
		}

		public Pig(World level) : base(level)
		{
			Height = 0.9;
			Width = 0.9;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 17 && entry is MetadataBool hasSaddle)
			{
				HasSaddle = hasSaddle.Value;
			}
		}
	}
}