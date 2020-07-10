using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Bat : PassiveMob
	{
		private bool _isHanging = false;

		public bool IsHanging
		{
			get
			{
				return _isHanging;
			}
			set
			{
				_isHanging = value;

				if (ModelRenderer != null)
				{
					
					//ModelRenderer
				}
			}
		}
		public Bat(World level) : base(EntityType.Bat, level)
		{
			JavaEntityId = 65;
			Height = 0.9;
			Width = 0.5;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 15 && entry is MetadataByte meta)
			{
				IsHanging = (meta.Value & 0x01) != 0;
			}
		}
	}
}
