using System.Collections.Generic;
using System.Linq;
using Alex.Interfaces;
using Alex.ResourcePackLib.Json.Converters.Bedrock;
using ConcreteMC.MolangSharp.Parser;
using ConcreteMC.MolangSharp.Runtime;
using ConcreteMC.MolangSharp.Runtime.Value;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.MoLang
{
	[JsonConverter(typeof(MoVec2Converter))]
	public class MoLangVector2Expression
	{
		private IExpression _x, _y;

		public MoLangVector2Expression(IExpression[] values)
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

		private IReadOnlyDictionary<double, AnimationChannelData> _keyFrames;

		public MoLangVector2Expression(Dictionary<string, AnimationChannelData> keyframes)
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

		private IVector2 Evaluate(MoLangRuntime runtime,
			IExpression xExpressions,
			IExpression yExpressions,
			IVector2 currentValue)
		{
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.X);
			IMoValue x = runtime.Execute(xExpressions);
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.Y);
			IMoValue y = runtime.Execute(yExpressions);

			return VectorUtils.VectorFactory.Vector2(x.AsFloat(), y.AsFloat());
		}

		private IVector2 Evaluate(MoLangRuntime runtime, IExpression[] expressions, IVector2 currentValue)
		{
			if (expressions.Length == 3)
			{
				return Evaluate(runtime, expressions[0], expressions[1], currentValue);
			}

			return Evaluate(runtime, expressions[0], expressions[0], currentValue);
		}

		private IVector2 Evaluate(MoLangRuntime runtime,
			AnimationChannelData complex,
			bool lookAHead,
			IVector2 currentValue)
		{
			if (complex == null)
				return VectorUtils.VectorFactory.Vector2(0,0);


			if (complex.Expressions != null)
			{
				var expressions = complex.Expressions;

				return Evaluate(runtime, expressions, currentValue);
			}

			if (lookAHead)
				return Evaluate(runtime, complex.KeyFrame.Pre, currentValue);

			return Evaluate(runtime, complex.KeyFrame.Post, currentValue);
		}

		public IVector2 Evaluate(MoLangRuntime runtime, IVector2 currentValue, double animationTime = 0d)
		{
			if (_keyFrames != null)
			{
				var elapsedTime = animationTime % _keyFrames.Max(x => x.Key);

				AnimationChannelData previous = null;
				double previousKey = 0d;
				AnimationChannelData next = null;
				double nextKey = 0d;

				foreach (var keyframe in _keyFrames.OrderBy(x => x.Key))
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
				IVector2 previousVector = Evaluate(runtime, previous, false, currentValue);
				IVector2 nextVector = Evaluate(runtime, next, true, currentValue);

				return VectorUtils.Lerp(previousVector, nextVector, (float)((1f / timeBetweenFrames) * accumulator));
			}

			return Evaluate(runtime, _x, _y, currentValue);
		}
	}
}