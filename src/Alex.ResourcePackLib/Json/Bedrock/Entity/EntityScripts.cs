using System;
using System.Collections.Generic;
using Alex.MoLang.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class EntityScripts
	{
		[JsonProperty("initialize")] 
		public List<IExpression>[] Initialize { get; set; } = new List<IExpression>[0];

		[JsonProperty("pre_animation")] 
		public List<IExpression>[] PreAnimation { get; set; } = new List<IExpression>[0];

		[JsonProperty("scale")] 
		public List<IExpression> Scale { get; set; } = null;

		[JsonProperty("parent_setup")]
		public List<IExpression> ParentSetup { get; set; } = null;

		[JsonProperty("animate")] 
		public AnnoyingMolangElement[] Animate { get; set; } = new AnnoyingMolangElement[0];
		
		[JsonProperty("should_update_bones_and_effects_offscreen")]
		public List<IExpression> ShouldUpdateBonesAndEffectsOffscreen { get; set; } = null;
		
		[JsonProperty("should_update_effects_offscreen")]
		public List<IExpression> ShouldUpdateEffectsOffscreen { get; set; } = null;
	}
	
	public struct AnnoyingMolangElement
	{
		public Dictionary<string, List<IExpression>> Expressions;
		public string                                StringValue;
		
		public static implicit operator AnnoyingMolangElement(Dictionary<string, List<IExpression>> dictionary) => new AnnoyingMolangElement { Expressions = dictionary };
		public static implicit operator AnnoyingMolangElement(string stringValue) => new AnnoyingMolangElement { StringValue = stringValue };
	}

	internal class AnimateElementConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(AnnoyingMolangElement) || t == typeof(AnnoyingMolangElement?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			switch (reader.TokenType)
			{
				case JsonToken.String:
				case JsonToken.Date:
					var stringValue = serializer.Deserialize<string>(reader);

					return new AnnoyingMolangElement {StringValue = stringValue};

				case JsonToken.StartObject:
					var objectValue = serializer.Deserialize<Dictionary<string, List<IExpression>>>(reader);

					return new AnnoyingMolangElement {Expressions = objectValue};
			}

			throw new Exception("Cannot unmarshal type AnimateElement");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			var value = (AnnoyingMolangElement) untypedValue;

			if (value.StringValue != null)
			{
				serializer.Serialize(writer, value.StringValue);

				return;
			}

			if (value.Expressions != null)
			{
				serializer.Serialize(writer, value.Expressions);

				return;
			}

			throw new Exception("Cannot marshal type AnimateElement");
		}
	}
}