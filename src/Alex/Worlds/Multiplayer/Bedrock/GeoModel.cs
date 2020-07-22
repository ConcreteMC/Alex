using System;
using System.Collections.Generic;
using System.Linq;
using Alex.ResourcePackLib.Json.Models.Entities;
using Newtonsoft.Json;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class GeometryModel
	{
		[JsonProperty(PropertyName = "format_version")]
		public string FormatVersion { get; set; } = "1.12.0";

		[JsonProperty(PropertyName = "minecraft:geometry")]
		public List<EntityModel> Geometry { get; set; } = new List<EntityModel>();

		public EntityModel FindGeometry(string geometryName, bool matchPartial = true)
		{
			string fullName = this.Geometry.FirstOrDefault((g => !matchPartial ? g.Description.Identifier.Equals(geometryName, StringComparison.InvariantCultureIgnoreCase) : g.Description.Identifier.StartsWith(geometryName, StringComparison.InvariantCultureIgnoreCase)))?.Description.Identifier;
			if (fullName == null)
				return null;
			var geometry = this.Geometry.First( (g => g.Description.Identifier == fullName));
			geometry.Name = fullName;
			return geometry;
		}

		public MiNET.Utils.Skins.Geometry CollapseToDerived(MiNET.Utils.Skins.Geometry derived)
		{
			if (derived == null)
				throw new ArgumentNullException(nameof (derived));
			return derived;
		}

		public object Clone()
		{
			GeometryModel geometryModel = (GeometryModel) this.MemberwiseClone();
			geometryModel.Geometry = new List<EntityModel>();
			foreach (var geometry in this.Geometry)
				geometryModel.Geometry.Add(geometry);
			return (object) geometryModel;
		}
	}
}