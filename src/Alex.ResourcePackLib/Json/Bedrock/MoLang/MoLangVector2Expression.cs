using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Converters.Bedrock;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.MoLang
{
	[JsonConverter(typeof(MoVec2Converter))]
	public class MoLangVector2Expression
	{
		private IExpression[] _x, _y;
		
		public MoLangVector2Expression(IExpression[][] values)
		{
			if (values.Length == 2)
			{
				_x = values[0];
				_y = values[1];
			}
			else if (values.Length == 1)
			{
				_x = _y = values[0];
			}
		}

		private IReadOnlyDictionary<double, ComplexStuff> _keyFrames;
		public MoLangVector2Expression(Dictionary<string, ComplexStuff> keyframes)
		{
			var newKeyFrames = new Dictionary<double, ComplexStuff>();

			foreach (var keyframe in keyframes)
			{
				if (double.TryParse(keyframe.Key, out double time))
				{
					newKeyFrames.TryAdd(time, keyframe.Value);
				}
			}
			
			_keyFrames = newKeyFrames;
		}

		private Vector2 Evaluate(MoLangRuntime runtime, IExpression[] xExpressions, IExpression[] yExpressions, Vector2 currentValue)
		{
			IMoValue x = runtime.Execute(xExpressions, new Dictionary<string, IMoValue>()
			{
				{"this", new DoubleValue(currentValue.X)}
			});
			IMoValue y = runtime.Execute(yExpressions, new Dictionary<string, IMoValue>()
			{
				{"this", new DoubleValue(currentValue.Y)}
			});

			return new Vector2(x.AsFloat(), y.AsFloat());
		}

		private Vector2 Evaluate(MoLangRuntime runtime, IExpression[][] expressions, Vector2 currentValue)
		{
			if (expressions.Length == 3)
			{
				return Evaluate(runtime, expressions[0], expressions[1], currentValue);
			}

			return Evaluate(runtime, expressions[0], expressions[0], currentValue);
		}
		
		private Vector2 Evaluate(MoLangRuntime runtime, ComplexStuff complex, bool lookAHead, Vector2 currentValue)
		{
			if (complex == null)
				return Vector2.Zero;

			
			if (complex.Expressions != null)
			{
				var expressions = complex.Expressions;
				return Evaluate(runtime, expressions, currentValue);
			}

			if (lookAHead)
				return Evaluate(runtime, complex.Frame.Pre, currentValue);
			
			return Evaluate(runtime, complex.Frame.Post, currentValue);
		}

		public Vector2 Evaluate(MoLangRuntime runtime, Vector2 currentValue, double animationTime = 0d)
		{
			if (_keyFrames != null)
			{
				var elapsedTime = animationTime % _keyFrames.Max(x => x.Key);

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
				Vector2 previousVector = Evaluate(runtime, previous, false, currentValue);
				Vector2 nextVector = Evaluate(runtime, next, true, currentValue);

				return Vector2.Lerp(previousVector, nextVector, (float) ((1f / timeBetweenFrames) * accumulator));
			}

			return Evaluate(runtime, _x, _y, currentValue);
		}
	}
}