using System;
using System.Collections.Generic;
using System.Linq;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class GeometryModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GeometryModel));
		
		[JsonProperty(PropertyName = "format_version")]
		public string FormatVersion { get; set; } = "1.12.0";

		[JsonProperty(PropertyName = "minecraft:geometry")]
		public List<EntityModel> Geometry { get; set; } = new List<EntityModel>();

		public EntityModel FindGeometry(string geometryName, bool matchPartial = true)
		{
			string fullName = this.Geometry.FirstOrDefault(
					(g => !matchPartial ?
						g.Description.Identifier.Equals(geometryName, StringComparison.InvariantCultureIgnoreCase) :
						g.Description.Identifier.StartsWith(geometryName, StringComparison.InvariantCultureIgnoreCase)))
			  ?.Description.Identifier;

			if (fullName == null)
				return null;

			var geometry = this.Geometry.First((g => g.Description.Identifier == fullName));

			return geometry;
		}

		public static bool TryParse(string json, SkinResourcePatch resourcePatch, out GeometryModel output)
		{
			try
			{
				JObject obj = JObject.Parse(json, new JsonLoadSettings());

				var entries = new List<EntityModel>();
				output = new GeometryModel();

				foreach (var e in obj)
				{
					try
					{
						if (e.Key == "format_version" && e.Value.Type == JTokenType.String)
						{
							output.FormatVersion = e.Value.ToObject<string>();
						}
						else if (e.Key == "minecraft:geometry" && e.Value.Type == JTokenType.Array)
						{
							var models = e.Value.ToObject<EntityModel[]>(MCJsonConvert.Serializer);

							if (models != null)
							{
								foreach (var model in models)
								{
									if (entries.Contains(model))
									{
										Log.Warn($"The name {model.Description.Identifier} was already in use!");
									}
									else
									{
										entries.Add(model);
									}
								}

								continue;
							}
						}

						if (!e.Key.StartsWith("geometry."))
						{
							if (e.Value.Type == JTokenType.Array)
							{
								continue;

								foreach (var type in e.Value.ToObject<OldEntityModel[]>(MCJsonConvert.Serializer))
								{
									//entries.TryAdd(e.Key, type);
								}
							}

							continue;
						}

						var newModel = e.Value.ToObject<EntityModel>(MCJsonConvert.Serializer);

						if (newModel != null)
						{
							if (string.IsNullOrWhiteSpace(newModel.Description?.Identifier))
							{
								newModel.Description.Identifier = e.Key;
							}

							if (!entries.Contains(newModel))
								entries.Add(newModel);
						}
					}
					catch (Exception ex)
					{
						Log.Warn(ex, $"Failed to decode geometry");
					}
				}

				if (entries.Count == 0)
				{
					output = null;

					return false;
				}

				output.Geometry = entries;

				return true;
			}
			catch(JsonReaderException)
			{
				output = null;
				return false;
			}
		}
	}
}