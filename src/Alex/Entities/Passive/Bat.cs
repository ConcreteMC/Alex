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

		public Bat(World level) : base(level)
		{
			Height = 0.9;
			Width = 0.5;

			IsAffectedByGravity = false;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataByte meta)
			{
				IsHanging = (meta.Value & 0x01) != 0;
			}
		}

		/// <inheritdoc />
		public override void EntityDied()
		{
			base.EntityDied();
			Alex.Instance.AudioEngine.PlaySound("mob.bat.death", RenderLocation, 1f, 1f);
		}

		/// <inheritdoc />
		public override void EntityHurt()
		{
			base.EntityHurt();
			Alex.Instance.AudioEngine.PlaySound("mob.bat.hurt", RenderLocation, 1f, 1f);
		}
	}
}