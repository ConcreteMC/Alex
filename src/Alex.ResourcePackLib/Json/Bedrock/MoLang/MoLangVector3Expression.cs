using System;
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
	/// <summary>
	///		Holds the actual keyframe data for a channel.
	///		(Rotation, Position, Scale etc)
	/// </summary>
	[JsonConverter(typeof(MoVec3Converter))]
	public class MoLangVector3Expression
	{
		private IExpression[] _x, _y, _z;
		
		public MoLangVector3Expression(IExpression[][] values)
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
					newKeyFrames.TryAdd(time, keyframe.Value);
				}
			}
			
			_keyFrames = newKeyFrames;
		}

		private Vector3 Evaluate(MoLangRuntime runtime, IExpression[] xExpressions, IExpression[] yExpressions, IExpression[] zExpressions, Vector3 currentValue)
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

			return new Vector3(x.AsFloat(), y.AsFloat(), z.AsFloat());
		}

		private Vector3 Evaluate(MoLangRuntime runtime, IExpression[][] expressions, Vector3 currentValue)
		{
			if (expressions.Length == 3)
			{
				return Evaluate(runtime, expressions[0], expressions[1], expressions[2], currentValue);
			}

			return Evaluate(runtime, expressions[0], expressions[0], expressions[0], currentValue);
		}
		
		private Vector3 Evaluate(MoLangRuntime runtime, ComplexStuff complex, bool lookAHead, Vector3 currentValue)
		{
			if (complex == null)
				return Vector3.Zero;

			
			if (complex.Expressions != null)
			{
				var expressions = complex.Expressions;
				return Evaluate(runtime, expressions, currentValue);
			}

			if (lookAHead)
				return Evaluate(runtime, complex.Frame.Pre, currentValue);
			
			return Evaluate(runtime, complex.Frame.Post, currentValue);
		}

		public Vector3 Evaluate(MoLangRuntime runtime, Vector3 currentValue, double animationTime = 0d)
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
				Vector3 previousVector = Evaluate(runtime, previous, false, currentValue);
				Vector3 nextVector = Evaluate(runtime, next, true, currentValue);

				var lerpTime = (float) ((1f / timeBetweenFrames) * accumulator);
				
				return Vector3.Lerp(previousVector, nextVector, lerpTime);
			}

			return Evaluate(runtime, _x, _y, _z, currentValue);
		}
	}
}