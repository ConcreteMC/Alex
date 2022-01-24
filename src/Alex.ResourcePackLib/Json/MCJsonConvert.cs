using System;
using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Converters.MoLang;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Alex.ResourcePackLib.Json
{
	public class MCJsonConvert
	{
		public const bool PrettyPrint = false;

		public static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings()
		{
			ContractResolver =
				new CamelCasePropertyNamesContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },
			NullValueHandling = NullValueHandling.Ignore,
			//DefaultValueHandling = DefaultValueHandling.,
			PreserveReferencesHandling = PreserveReferencesHandling.None
		};

		public static readonly JsonSerializer Serializer;

		static MCJsonConvert()
		{
			DefaultSettings.Converters.Add(new Vector3Converter());
			DefaultSettings.Converters.Add(new Vector2Converter());
			//DefaultSettings.Converters.Add(new SingleOrArrayConverter<MultiPartRule>());
			DefaultSettings.Converters.Add(new BlockStateMultipartRuleConverter());
			DefaultSettings.Converters.Add(new GuidConverter());
			//DefaultSettings.Converters.Add(new MCElementsDictionaryConverter());
			DefaultSettings.Converters.Add(new EntityModelConverter());
			DefaultSettings.Converters.Add(new MobsModelConverter());
			DefaultSettings.Converters.Add(new UvConverter());
			DefaultSettings.Converters.Add(new FontDefinitionConverter());
			DefaultSettings.Converters.Add(new MoLangExpressionConverter());
			DefaultSettings.Converters.Add(new AnnoyingMolangElementConverter());
			DefaultSettings.Converters.Add(new ResourceLocationConverter());
			DefaultSettings.Converters.Add(new BlockVariantKeyConverter());
			Serializer = JsonSerializer.Create(DefaultSettings);
		}

		public static string SerializeObject(object obj, bool prettyPrint = PrettyPrint)
		{
			return JsonConvert.SerializeObject(
				obj, prettyPrint ? Formatting.Indented : Formatting.None, DefaultSettings);
		}

		public static T DeserializeObject<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json, DefaultSettings);
		}

		public static T DeserializeObject<T>(string json, params JsonConverter[] converters)
		{
			var cv = new List<JsonConverter>();
			cv.AddRange(DefaultSettings.Converters);
			cv.AddRange(converters);

			return JsonConvert.DeserializeObject<T>(
				json,
				new JsonSerializerSettings()
				{
					Converters = cv,
					ContractResolver = DefaultSettings.ContractResolver,
					NullValueHandling = DefaultSettings.NullValueHandling,
					PreserveReferencesHandling = DefaultSettings.PreserveReferencesHandling
				});
		}

		public static object DeserializeObject(string json)
		{
			return JsonConvert.DeserializeObject(json, DefaultSettings);
		}

		public static object DeserializeObject(string json, Type type)
		{
			return JsonConvert.DeserializeObject(json, type, DefaultSettings);
		}

		public static TOutput PopulateObject<TOutput>(TOutput output, string json)
		{
			var clone = DeserializeObject<TOutput>(SerializeObject(output));
			JsonConvert.PopulateObject(json, clone, DefaultSettings);

			return clone;
		}

		public static TOutput PopulateObject<TOutput, TInput>(TOutput output, TInput input)
		{
			var clone = PopulateObject(output, SerializeObject(input));

			return clone;
		}
	}
}