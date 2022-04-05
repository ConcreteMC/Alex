using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Interfaces;
using Alex.ResourcePackLib.Json.Converters.Bedrock;
using ConcreteMC.MolangSharp.Parser;
using ConcreteMC.MolangSharp.Parser.Expressions;
using ConcreteMC.MolangSharp.Runtime;
using ConcreteMC.MolangSharp.Runtime.Value;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.MoLang
{
	[JsonConverter(typeof(MoVec4Converter))]
	public class MoLangVector4Expression
	{
		private IExpression _x, _y, _z, _w;

		public MoLangVector4Expression(IExpression[] values)
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

		private IVector4 Evaluate(MoLangRuntime runtime,
			IExpression xExpressions,
			IExpression yExpressions,
			IExpression zExpressions,
			IExpression wExpressions,
			IVector4 currentValue)
		{
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.X);
			IMoValue x = runtime.Execute(xExpressions);
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.Y);
			IMoValue y = runtime.Execute(yExpressions);
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.Z);
			IMoValue z = runtime.Execute(zExpressions);
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.W);
			IMoValue w = runtime.Execute(wExpressions);

			return VectorUtils.VectorFactory.Vector4(x.AsFloat(), y.AsFloat(), z.AsFloat(), w.AsFloat());
		}

		private IVector4 Evaluate(MoLangRuntime runtime, IExpression[] expressions, IVector4 currentValue)
		{
			if (expressions.Length == 4)
			{
				return Evaluate(runtime, expressions[0], expressions[1], expressions[2], expressions[3], currentValue);
			}

			if (expressions.Length == 3)
			{
				return Evaluate(
					runtime, expressions[0], expressions[1], expressions[2],
					new NumberExpression(1d), currentValue);
			}

			return Evaluate(runtime, expressions[0], expressions[0], expressions[0], expressions[0], currentValue);
		}

		private IVector4 Evaluate(MoLangRuntime runtime,
			AnimationChannelData complex,
			bool lookAHead,
			IVector4 currentValue)
		{
			if (complex == null)
				return VectorUtils.VectorFactory.Vector4(0,0,0,0);


			if (complex.Expressions != null)
			{
				var expressions = complex.Expressions;

				return Evaluate(runtime, expressions, currentValue);
			}

			if (lookAHead)
				return Evaluate(runtime, complex.KeyFrame.Pre, currentValue);

			return Evaluate(runtime, complex.KeyFrame.Post, currentValue);
		}

		public IVector4 Evaluate(MoLangRuntime runtime,
			IVector4 currentValue,
			double interpolator = -1d,
			double animationTime = 0d)
		{
			if (_keyFrames != null)
			{
				var elapsedTime = (interpolator >= 0d ? interpolator : animationTime) % _keyFrames.Max(x => x.Key);

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
				IVector4 previousVector = Evaluate(runtime, previous, false, currentValue);
				IVector4 nextVector = Evaluate(runtime, next, true, currentValue);

				return VectorUtils.Lerp(previousVector, nextVector, (float)((1f / timeBetweenFrames) * accumulator));
			}

			return Evaluate(runtime, _x, _y, _z, _w, currentValue);
		}
	}
}