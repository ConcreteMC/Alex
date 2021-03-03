using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Converters.MoLang;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class Animation
	{
		[JsonProperty("loop")]
		public bool Loop { get; set; } = false;

		[JsonProperty("override_previous_animation")]
		public bool OverridePreviousAnimation { get; set; } = false;
		
		[JsonProperty("bones")]
		public Dictionary<string, AnimationBoneElement> Bones { get; set; } =
			new Dictionary<string, AnimationBoneElement>();
	}

	public class AnimationBoneElement
	{
		[JsonProperty("position")]
		public MoLangVector3Expression Position { get; set; }
		
		[JsonProperty("rotation")]
		public MoLangVector3Expression Rotation { get; set; }
		
		[JsonProperty("scale")]
		public MoLangVector3Expression Scale { get; set; }
	}

	[JsonConverter(typeof(MoVec3Converter))]
	public class MoLangVector3Expression
	{
		private List<IExpression> _x, _y, _z;
		
		public MoLangVector3Expression(List<IExpression>[] values)
		{
			if (values.Length == 3)
			{
				_x = values[0];
				_y = values[1];
				_z = values[2];
			}
			else if (values.Length == 1)
			{
				_x = _y = _z = values[0];
			}
		}

		private IReadOnlyDictionary<double, ComplexStuff> _keyFrames;
		public MoLangVector3Expression(Dictionary<string, ComplexStuff> keyframes)
		{
			var newKeyFrames = new Dictionary<double, ComplexStuff>();

			foreach (var keyframe in keyframes)
			{
				if (double.TryParse(keyframe.Key, out double time))
				{
					newKeyFrames.Add(time, keyframe.Value);
				}
			}
			
			_keyFrames = newKeyFrames;
		}

		private Vector3 Evaluate(MoLangRuntime runtime, List<IExpression> xExpressions, List<IExpression> yExpressions, List<IExpression> zExpressions)
		{
			IMoValue x = runtime.Execute(xExpressions);
			IMoValue y = runtime.Execute(yExpressions);
			IMoValue z = runtime.Execute(zExpressions);

			return new Vector3(x.AsFloat(), y.AsFloat(), z.AsFloat());
		}

		private Vector3 Evaluate(MoLangRuntime runtime, List<IExpression>[] expressions)
		{
			if (expressions.Length == 3)
			{
				return Evaluate(runtime, expressions[0], expressions[1], expressions[2]);
			}

			var val = runtime.Execute(expressions[0]);

			return new Vector3(val.AsFloat());
		}
		
		private Vector3 Evaluate(MoLangRuntime runtime, ComplexStuff complex, bool lookAHead)
		{
			if (complex == null)
				return Vector3.Zero;

			
			if (complex.Expressions != null)
			{
				var expressions = complex.Expressions;
				return Evaluate(runtime, expressions);// new Vector3(val.AsFloat());
			}

			if (lookAHead)
				return Evaluate(runtime, complex.Frame.Pre);
			
			return Evaluate(runtime, complex.Frame.Post);
			//if (complex.Frame.)
			//IMoValue x = runtime.Execute(xExpressions);
			//IMoValue y = runtime.Execute(yExpressions);
			//IMoValue z = runtime.Execute(zExpressions);

			//return new Vector3(x.AsFloat(), y.AsFloat(), z.AsFloat());
		}

		public Vector3 Evaluate(MoLangRuntime runtime)
		{
			if (_keyFrames != null)
			{
				var elapsedTime = runtime.Environment.GetValue("query.life_time").AsDouble() % _keyFrames.Max(x => x.Key);

				ComplexStuff previous = null;
				double previousKey = 0d;
				ComplexStuff next = null;
				double nextKey = 0d;
				foreach (var keyframe in _keyFrames.OrderBy(x=> x.Key))
				{
					if (keyframe.Key >= elapsedTime)
					{
						next = keyframe.Value;
						nextKey = keyframe.Key;

						break;
					}
					else if (keyframe.Key <= elapsedTime)
					{
						previous = keyframe.Value;
						previousKey = keyframe.Key;
					}
				}

				var timeBetweenFrames = (nextKey - previousKey);
				var accumulator = elapsedTime - previousKey;
				Vector3 previousVector = Evaluate(runtime, previous, false);
				Vector3 nextVector = Evaluate(runtime, next, true);

				return Vector3.Lerp(previousVector, nextVector, (float) ((1f / timeBetweenFrames) * accumulator));
			}

			return Evaluate(runtime, _x, _y, _z);
		}
	}

	public class MoVec3Converter : JsonConverter<MoLangVector3Expression>
	{
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, MoLangVector3Expression value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override MoLangVector3Expression ReadJson(JsonReader reader,
			Type objectType,
			MoLangVector3Expression existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			switch (obj.Type)
			{
				case JTokenType.Array:
					if (obj is JArray jArray)
					{
						List<IExpression>[] values = jArray.ToObject<List<IExpression>[]>(MCJsonConvert.Serializer);

						return new MoLangVector3Expression(values);
					}
					break;
				case JTokenType.Object:
					if (obj is JObject jObject)
					{
						return new MoLangVector3Expression(
							jObject.ToObject<Dictionary<string, ComplexStuff>>(
								new JsonSerializer()
								{
									Converters = { new MoLangExpressionConverter()}
								}));
					}
					break;
			}
			
			List<IExpression> raw = obj.ToObject<List<IExpression>>(MCJsonConvert.Serializer);

			return new MoLangVector3Expression(new List<IExpression>[1]
			{
				raw
			});
			
			throw new Exception("No.");
		}
	}

	[JsonConverter(typeof(ComplexStuffConverter))]
	public class ComplexStuff
	{
		public List<IExpression>[] Expressions { get; set; }
		public PrePostKeyFrame Frame { get; set; }
	}

	public class PrePostKeyFrame
	{
		[JsonProperty("pre")]
		public List<IExpression>[] Pre { get; set; }
		
		[JsonProperty("post")]
		public List<IExpression>[] Post { get; set; }
	}
	
	internal class ComplexStuffConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(ComplexStuff);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);
			
			switch (obj.Type)
			{
				case JTokenType.Array:
					var expressions = obj.ToObject<List<IExpression>[]>(serializer);// serializer.Deserialize<List<IExpression>[]>(reader);

					return new ComplexStuff()
					{
						Expressions = expressions
					};

				case JTokenType.Object:
					var frameValue = obj.ToObject<PrePostKeyFrame>(serializer);// serializer.Deserialize<PrePostKeyFrame>(reader);

					return new ComplexStuff()
					{
						Frame = frameValue
					};
			}

			throw new Exception("Cannot unmarshal type AnimateElement");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			
			throw new Exception("Cannot marshal type ComplexStuff");
		}
	}
}