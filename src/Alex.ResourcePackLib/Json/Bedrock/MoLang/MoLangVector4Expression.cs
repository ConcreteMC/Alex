using System;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Converters.Bedrock;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.MoLang
{
	[JsonConverter(typeof(MoVec4Converter))]
	public class MoLangVector4Expression
	{
		private IExpression[] _x, _y, _z, _w;
		
		public MoLangVector4Expression(IExpression[][] values)
		{
			if (values.Length == 4)
			{
				_x = values[0];
				_y = values[1];
				_z = values[2];
				_w = values[3];
			}
			else if (values.Length == 1)
			{
				_x = _y = _z = _w = values[0];
			}
		}

		private IReadOnlyDictionary<double, AnimationChannelData> _keyFrames;
		public MoLangVector4Expression(Dictionary<string, AnimationChannelData> keyframes)
		{
			var newKeyFrames = new Dictionary<double, AnimationChannelData>();

			foreach (var keyframe in keyframes)
			{
				if (double.TryParse(keyframe.Key, out double time))
				{
					newKeyFrames.TryAdd(time, keyframe.Value);
				}
			}
			
			_keyFrames = newKeyFrames;
		}

		private Vector4 Evaluate(MoLangRuntime runtime, IExpression[] xExpressions, IExpression[] yExpressions, IExpression[] zExpressions, IExpression[] wExpressions, Vector4 currentValue)
		{
			IMoValue x = runtime.Execute(xExpressions, new Dictionary<string, IMoValue>(StringComparer.OrdinalIgnoreCase)
			{
				{"this", new DoubleValue(currentValue.X)}
			});
			IMoValue y = runtime.Execute(yExpressions, new Dictionary<string, IMoValue>(StringComparer.OrdinalIgnoreCase)
			{
				{"this", new DoubleValue(currentValue.Y)}
			});
			IMoValue z = runtime.Execute(zExpressions, new Dictionary<string, IMoValue>(StringComparer.OrdinalIgnoreCase)
			{
				{"this", new DoubleValue(currentValue.Z)}
			});
			IMoValue w = runtime.Execute(wExpressions, new Dictionary<string, IMoValue>(StringComparer.OrdinalIgnoreCase)
			{
				{"this", new DoubleValue(currentValue.W)}
			});

			return new Vector4(x.AsFloat(), y.AsFloat(), z.AsFloat(), w.AsFloat());
		}

		private Vector4 Evaluate(MoLangRuntime runtime, IExpression[][] expressions, Vector4 currentValue)
		{
			if (expressions.Length == 4)
			{
				return Evaluate(runtime, expressions[0], expressions[1], expressions[2], expressions[3], currentValue);
			}

			if (expressions.Length == 3)
			{
				return Evaluate(runtime, expressions[0], expressions[1], expressions[2], new IExpression[]
				{
					new NumberExpression(1d)
				}, currentValue);
			}
			
			return Evaluate(runtime, expressions[0], expressions[0], expressions[0], expressions[0], currentValue);
		}
		
		private Vector4 Evaluate(MoLangRuntime runtime, AnimationChannelData complex, bool lookAHead, Vector4 currentValue)
		{
			if (complex == null)
				return Vector4.Zero;

			
			if (complex.Expressions != null)
			{
				var expressions = complex.Expressions;
				return Evaluate(runtime, expressions, currentValue);
			}

			if (lookAHead)
				return Evaluate(runtime, complex.KeyFrame.Pre, currentValue);
			
			return Evaluate(runtime, complex.KeyFrame.Post, currentValue);
		}

		public Vector4 Evaluate(MoLangRuntime runtime, Vector4 currentValue, double interpolator = -1d, double animationTime = 0d)
		{
			if (_keyFrames != null)
			{
				var elapsedTime = (interpolator >= 0d ? interpolator :animationTime) % _keyFrames.Max(x => x.Key);

				AnimationChannelData previous = null;
				double previousKey = 0d;
				AnimationChannelData next = null;
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
				Vector4 previousVector = Evaluate(runtime, previous, false, currentValue);
				Vector4 nextVector = Evaluate(runtime, next, true, currentValue);

				return Vector4.Lerp(previousVector, nextVector, (float) ((1f / timeBetweenFrames) * accumulator));
			}

			return Evaluate(runtime, _x, _y, _z, _w, currentValue);
		}
	}
}