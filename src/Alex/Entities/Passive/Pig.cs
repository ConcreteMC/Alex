using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Pig : PassiveMob
	{
		private bool _hasSaddle;

		public bool HasSaddle
		{
			get => _hasSaddle;
			set
			{
				_hasSaddle = value;
				
				var modelRenderer = ModelRenderer;

				if (modelRenderer != null && modelRenderer.Valid)
				{
					if (modelRenderer.GetBone("Bag1", out var bag1))
					{
						ToggleCubes(bag1, !IsChested);
					}
				}
			}
		}

		public Pig(World level) : base((EntityType)12, level)
		{
			JavaEntityId = 90;
			Height = 0.9;
			Width = 0.9;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataBool hasSaddle)
			{
				HasSaddle = hasSaddle.Value;
			}
		}
	}
}
