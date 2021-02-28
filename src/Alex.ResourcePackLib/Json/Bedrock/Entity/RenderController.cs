using System;
using System.Collections.Generic;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Expressions;
using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	internal class VersionedResourceConverter<T> : JsonConverter<VersionedResource<T>>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(VersionedResourceConverter<T>));
		
		private string ValuesProperty { get; }
		public VersionedResourceConverter(string valuesProperty)
		{
			ValuesProperty = valuesProperty;
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, VersionedResource<T> value, JsonSerializer serializer)
		{
			
		}

		/// <inheritdoc />
		public override VersionedResource<T> ReadJson(JsonReader reader,
			Type objectType,
			VersionedResource<T> existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type != JTokenType.Object) 
				return null;

			string              formatVersion = "1.8.0";
			var                 jObject       = (JObject)obj;
			VersionedResource<T> result        = new VersionedResource<T>();
			if (jObject.TryGetValue(
				"format_version", StringComparison.InvariantCultureIgnoreCase, out var versionToken))
			{
				string format = versionToken.Value<string>();
				formatVersion = format;
			}

			result.FormatVersion = FormatVersionHelpers.FromString(formatVersion);
			
			if (jObject.TryGetValue(ValuesProperty, out var values))
			{
				if (values.Type == JTokenType.Object)
				{
					foreach (var property in (JObject)values)
					{
						if (!result.TryAdd(property.Key, property.Value.ToObject<T>(serializer)))
						{
							Log.Warn($"Duplicate key: {property.Key}");
						}
					}
				}
			}
			
			return result;
		}

		/// <inheritdoc />
		public override bool CanWrite { get; } = false;

		/// <inheritdoc />
	//	public override bool CanConvert(Type objectType)
	//	{
	//		return typeof(VersionedResource<T>).IsAssignableFrom(objectType);
	//	}
	}
	
	public class RenderController
	{
		[JsonProperty("geometry")] public string Geometry { get; set; } = null;

		[JsonProperty("rebuild_animation_matrices")]
		public bool RebuildAnimationMatrices { get; set; } = false;

		[JsonProperty("part_visibility")]
		public AnnoyingMolangElement[] PartVisibility { get; set; } = new AnnoyingMolangElement[]
		{
			new AnnoyingMolangElement(new Dictionary<string, List<IExpression>>()
			{
				{"*", new List<IExpression>() {new BooleanExpression(true)}}
			})
		};

		[JsonProperty("materials")] public AnnoyingMolangElement[] Materials { get; set; } = new AnnoyingMolangElement[]
		{
			new AnnoyingMolangElement(new Dictionary<string, List<IExpression>>()
			{
				{"*", new List<IExpression>()
				{
					new StringExpression("Material.default")
				}}
			})
		};

		[JsonProperty("textures")]
		public AnnoyingMolangElement[] Textures { get; set; } = new AnnoyingMolangElement[]
		{
			new AnnoyingMolangElement("Texture.default")
		};
	}

	public class PartVisibility : Dictionary<string, List<IExpression>>
	{
		
	}
	
	//public class Materials : AnnoyingMolangElement
	//{
		
	//}
}