using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Tags
{
    public class Tag
    {
		[JsonIgnore]
		public string Name { get; set; }

		[JsonIgnore]
		public string Namespace { get; set; }

		public bool Replace { get; set; }
		public string[] Values { get; set; } = new string[0];
    }
}
