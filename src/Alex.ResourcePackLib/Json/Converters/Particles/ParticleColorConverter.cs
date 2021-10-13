using System;
using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Alex.ResourcePackLib.Json.Bedrock.Particles.Components;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters.Particles
{
	[JsonConverter(typeof(ParticleColorConverter))]
	public class ParticleColor
	{
		private Color? _value = null;
		private GradientColors _gradientColors = null;
		private MoLangVector4Expression _expression = null;

		public ParticleColor(MoLangVector4Expression expression)
		{
			_expression = expression;
		}
		
		public ParticleColor(GradientColors gradientColors)
		{
			_gradientColors = gradientColors;
		}

		public ParticleColor(Color color)
		{
			_value = color;
		}

		public Color GetValue(MoLangRuntime runtime)
		{
			if (_value.HasValue)
				return _value.Value;

			if (_expression != null)
				return new Color(_expression.Evaluate(runtime, new Vector4(1, 1, 1, 1f)));
			
			if (_gradientColors != null)
				return _gradientColors.GetValue(runtime);

			return Color.White;
		}
	}

	public class GradientColors
	{
		[JsonProperty("gradient")]
		public MoLangVector4Expression[] Gradient { get; set; }
		
		[JsonProperty("interpolant")]
		public IExpression[] Interpolant { get; set; }

		public Color GetValue(MoLangRuntime runtime)
		{
			return Color.White;;
			//var interpolationValue = runtime.Execute(Interpolant).AsDouble();
			//return 
		}
	}
	
	public class ParticleColorConverter : JsonConverter<ParticleColor>
	{
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, ParticleColor value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override ParticleColor ReadJson(JsonReader reader,
			Type objectType,
			ParticleColor existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.Object)
			{
				var gradientValue = ((JObject) obj).ToObject<GradientColors>(serializer);

				return new ParticleColor(gradientValue);
			}
			else if (obj.Type == JTokenType.Array)
			{
				var expressions = obj.ToObject<IExpression[][]>(serializer);

				return new ParticleColor(new MoLangVector4Expression(expressions));
			}
			/*else if (obj.Type == JTokenType.String)
			{
				return new ParticleColor(new Color(obj.ToString()))
			}*/

			return new ParticleColor(Color.White);
		}
	}
}