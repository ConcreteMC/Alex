using System;
using System.Collections.Generic;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Expressions;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.Particles.Components;
using Alex.ResourcePackLib.Json.Converters.MoLang;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters.Particles
{
	public class ParticleComponentConverter : JsonConverter
	{
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			Dictionary<string, ParticleComponent> components = new Dictionary<string, ParticleComponent>();

			var obj = JToken.Load(reader);

			if (obj.Type != JTokenType.Object) 
				return null;

			var jObj = (JObject) obj;

			foreach (var kvp in jObj)
			{
				if (kvp.Value == null)
					continue;
				switch (kvp.Key)
				{
					case "minecraft:particle_appearance_billboard":
						components.Add(kvp.Key, kvp.Value.ToObject<AppearanceComponent>(serializer));
						break;
					case "minecraft:particle_motion_dynamic":
						components.Add(kvp.Key, kvp.Value.ToObject<MotionComponent>(serializer));
						break;
					case "minecraft:emitter_rate_manual":
						components.Add(kvp.Key, kvp.Value.ToObject<EmitterRateComponent>(serializer));
						break;
					case "minecraft:particle_lifetime_expression":
						components.Add(kvp.Key, kvp.Value.ToObject<LifetimeExpressionComponent>(serializer));
						break;
					case "minecraft:particle_appearance_tinting":
						components.Add(kvp.Key, kvp.Value.ToObject<AppearanceTintingComponent>(serializer));
						break;
					case "minecraft:particle_initial_speed":
						var s = new JsonSerializer();
						foreach (var serializerConverter in serializer.Converters)
						{
							s.Converters.Add(serializerConverter);
						}
						s.Converters.Add(new InitialSpeedConverter());
						
						components.Add(kvp.Key, kvp.Value.ToObject<InitialSpeedComponent>(s));
						break;
				}
			}
			
			return components;
		}

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(Dictionary<string,ParticleComponent>).IsAssignableFrom(objectType);
		}
	}

	public class InitialSpeedConverter : JsonConverter<InitialSpeedComponent>
	{
		/// <inheritdoc />
		public override bool CanWrite => false;
		
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, InitialSpeedComponent value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override InitialSpeedComponent ReadJson(JsonReader reader,
			Type objectType,
			InitialSpeedComponent existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var token = JToken.Load(reader);

			switch (token.Type)
			{
				case JTokenType.Array:
				{
					if (token is JArray jArray)
					{
						IExpression[][] values = jArray.ToObject<IExpression[][]>(MCJsonConvert.Serializer);

						return new InitialSpeedComponent() {Value = new MoLangVector3Expression(values)};
					}	
				} break;
				case JTokenType.Object:
					if (token is JObject jObject)
					{
						return new InitialSpeedComponent()
						{
							Value = new MoLangVector3Expression(
								jObject.ToObject<Dictionary<string, ComplexStuff>>(
									new JsonSerializer() {Converters = {new MoLangExpressionConverter()}}))
						};
					}
					break;
				case JTokenType.Integer:
					return new InitialSpeedComponent() {Value = new MoLangVector3Expression(new IExpression[][]
					{
						new IExpression[]
						{
							new NumberExpression(token.Value<int>())
						}
					})};

				case JTokenType.Float:
					return new InitialSpeedComponent() {Value = new MoLangVector3Expression(new IExpression[][]
					{
						new IExpression[]
						{
							new NumberExpression(token.Value<float>())
						}
					})};
			}
			
			return new InitialSpeedComponent() {Value = new MoLangVector3Expression(new IExpression[][]
			{
				new IExpression[]
				{
					new NumberExpression(0d)
				}
			})};
		}
	}
}