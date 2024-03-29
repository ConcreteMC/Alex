using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class EntityDescription
	{
		[JsonProperty("identifier")] public string Identifier { get; set; }

		[JsonIgnore] public FormatVersion FormatVersion { get; set; } = FormatVersion.Unknown;

		[JsonIgnore] public FormatVersion? MinEngine { get; set; }

		[JsonProperty("min_engine_version")] public string MinEngineVersion { get; set; }

		[JsonProperty("materials")] public Dictionary<string, string> Materials { get; set; }

		[JsonProperty("textures")] public Dictionary<string, string> Textures { get; set; }

		[JsonProperty("geometry")] public Dictionary<string, string> Geometry { get; set; }

		[JsonProperty("scripts")] public EntityScripts Scripts { get; set; }

		[JsonProperty("animations")] public Dictionary<string, string> Animations { get; set; }

		[JsonProperty("render_controllers")] public AnnoyingMolangElement[] RenderControllers { get; set; }

		[JsonProperty("animation_controllers")]
		public AnnoyingMolangElement[] AnimationControllers { get; set; }
	}
}