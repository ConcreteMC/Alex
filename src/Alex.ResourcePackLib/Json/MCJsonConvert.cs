using System;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Alex.ResourcePackLib.Json
{
	public class MCJsonConvert
	{
		public const bool PrettyPrint = false;

		public static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings()
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
			{
				NamingStrategy = new CamelCaseNamingStrategy()
			},

			NullValueHandling = NullValueHandling.Ignore,
			//DefaultValueHandling = DefaultValueHandling.,
			PreserveReferencesHandling = PreserveReferencesHandling.None
		};

		static MCJsonConvert()
		{
			DefaultSettings.Converters.Add(new Vector3Converter());
			DefaultSettings.Converters.Add(new Vector2Converter());
			//DefaultSettings.Converters.Add(new SingleOrArrayConverter<MultiPartRule>());
			DefaultSettings.Converters.Add(new BlockStateMultipartRuleConverter());
			DefaultSettings.Converters.Add(new GuidConverter());
			//DefaultSettings.Converters.Add(new MCElementsDictionaryConverter());
		}

		public static string SerializeObject(object obj, bool prettyPrint = PrettyPrint)
		{
			return JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None, DefaultSettings);
		}

		public static T DeserializeObject<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json, DefaultSettings);
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
