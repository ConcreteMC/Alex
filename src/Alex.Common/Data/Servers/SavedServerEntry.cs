using System;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Alex.Common.Data.Servers
{
	public class SavedServerEntry
	{
		public string ServerType { get; set; } = "Java";

		public string Name { get; set; }

		public string Host { get; set; }
		public ushort Port { get; set; }

		public Texture2D CachedIcon { get; set; }

		[JsonProperty("IntenalIdentifier")] public Guid InternalIdentifier = Guid.NewGuid();
	}
}