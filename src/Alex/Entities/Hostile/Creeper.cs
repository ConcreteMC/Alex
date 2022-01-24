using Alex.Graphics.Models.Entity;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Worlds;
using MiNET.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Alex.Entities.Hostile
{
	public class Creeper : HostileMob
	{
		private bool _isCharged = false;

		public bool IsCharged
		{
			get
			{
				return _isCharged;
			}
			set
			{
				_isCharged = value;
				//TryUpdateGeometry(
				//	"minecraft:creeper", value ? "charged" : "default", value ? "charged" : "default");
			}
		}

		public Creeper(World level) : base(level)
		{
			Height = 1.7;
			Width = 0.6;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataVarInt state) { }
			else if (entry.Index == 17 && entry is MetadataBool charged)
			{
				IsCharged = charged.Value;
			}
			else if (entry.Index == 18 && entry is MetadataBool ignited)
			{
				IsIgnited = ignited.Value;
			}
		}

		/// <inheritdoc />
		public override void EntityHurt()
		{
			base.EntityHurt();
			Alex.Instance.AudioEngine.PlaySound("mob.creeper.hurt", RenderLocation, 1f, 1f);
		}
	}
}