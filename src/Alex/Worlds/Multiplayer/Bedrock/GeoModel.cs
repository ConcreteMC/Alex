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
		
		private static JsonSerializer serializer = new JsonSerializer()
		{
			Converters = {new Vector3Converter(), new Vector2Converter()}
		};

		
		public static bool TryParse(string json, SkinResourcePatch resourcePatch, out GeometryModel output)
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
						var models = e.Value.ToObject<EntityModel[]>(serializer);

						if (models != null)
						{
							foreach (var model in models)
							{
								model.Name = model.Description.Identifier;
								model.Textureheight = model.Description.TextureHeight;
								model.Texturewidth = model.Description.TextureWidth;
								model.VisibleBoundsHeight = model.Description.VisibleBoundsHeight;
								model.VisibleBoundsWidth = model.Description.VisibleBoundsWidth;
								model.VisibleBoundsOffset = model.Description.VisibleBoundsOffset;

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

					if ( /*e.Key == "format_version" || e.Value.Type == JTokenType.Array*/
						!e.Key.StartsWith("geometry."))
					{
						if (e.Value.Type == JTokenType.Array)
						{
							continue;

							foreach (var type in e.Value.ToObject<EntityModel[]>(serializer))
							{
								//entries.TryAdd(e.Key, type);
							}
						}

						continue;
					}

					//if (e.Key == "minecraft:client_entity") continue;
					//if (e.Key.Contains("zombie")) Console.WriteLine(e.Key);
					var newModel = e.Value.ToObject<EntityModel>(serializer);

					if (newModel != null)
					{
						newModel.Name = e.Key;

						if (newModel.Description?.Identifier == null)
						{
							newModel.Description = new ModelDescription()
							{
								Identifier = e.Key,
								TextureHeight = newModel.Textureheight,
								TextureWidth = newModel.Texturewidth
							};
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
	}
}