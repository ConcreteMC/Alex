using Alex.API.Utils;
using Alex.Graphics.Models.Entity;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Llama : ChestedHorse
	{
		public Llama(World level) : base((EntityType)29, level)
		{
			JavaEntityId = 103;
			Height = 1.87;
			Width = 0.9;
		}

		public void SetVariant(int variant)
		{
			var texture = "creamy";
			switch (variant)
			{
				case 0:
					texture = "creamy";
					break;
				case 1:
					texture = "white";
					break;
				case 2:
					texture = "brown";
					break;
				case 3:
					texture = "gray";
					break;
				default:
					return;
			}

			TryUpdateTexture("minecraft::llama", texture);
		}
		
		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 21 && entry is MetadataVarInt variant)
			{
				SetVariant(variant.Value);
			}
		}
	}
}
