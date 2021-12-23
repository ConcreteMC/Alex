using System;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;
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
			IsKeyFramed = false;
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

		public bool IsKeyFramed { get; }
		public IReadOnlyDictionary<double, AnimationChannelData> KeyFrames => _keyFrames;

		private IReadOnlyDictionary<double, AnimationChannelData> _keyFrames;
		public MoLangVector3Expression(Dictionary<string, AnimationChannelData> keyframes)
		{
			IsKeyFramed = true;
			var newKeyFrames = new SortedDictionary<double, AnimationChannelData>();

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
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.X);
			IMoValue x = runtime.Execute(xExpressions);
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.Y);
			IMoValue y = runtime.Execute(yExpressions);
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.Z);
			IMoValue z = runtime.Execute(zExpressions);

			return new Vector3(x.AsFloat(), y.AsFloat(), z.AsFloat());
		}

		private Vector3 Evaluate(MoLangRuntime runtime, IExpression[][] expressions, Vector3 currentValue)
		{
			if (expressions == null) return currentValue;
			if (expressions.Length == 3)
			{
				return Evaluate(runtime, expressions[0], expressions[1], expressions[2], currentValue);
			}

			return Evaluate(runtime, expressions[0], expressions[0], expressions[0], currentValue);
		}
		
		private Vector3 Evaluate(MoLangRuntime runtime, AnimationChannelData complex, bool isPre, Vector3 currentValue)
		{
			if (complex == null)
				return currentValue;

			
			if (complex.Expressions != null)
			{
				var expressions = complex.Expressions;
				return Evaluate(runtime, expressions, currentValue);
			}

			if (isPre)
				return Evaluate(runtime, complex.KeyFrame.Pre, currentValue);
			
			return Evaluate(runtime, complex.KeyFrame.Post, currentValue);
		}

		public Vector3 Evaluate(MoLangRuntime runtime,
			Vector3 currentValue,
			double animationTime = 0d)
		{
			return Evaluate(runtime, currentValue, out _, animationTime);
		}

		public Vector3 Evaluate(MoLangRuntime runtime, Vector3 currentValue, out double timeRemaining, double elapsedTime = 0d)
		{
			timeRemaining = -1;
			if (_keyFrames == null) return Evaluate(runtime, _x, _y, _z, currentValue);
			

			AnimationChannelData previous = null;
			double previousKey = 0d;
			AnimationChannelData next = null;
			double nextKey = 0d;

			double difference = double.MaxValue;
			foreach (var keyframe in _keyFrames)
			{
				var diff = keyframe.Key - elapsedTime;
				if (keyframe.Key >= previousKey && keyframe.Key <= elapsedTime)
				{
					previousKey = keyframe.Key;
					previous = keyframe.Value;
				}
				else if (diff >= 0d && diff < difference)
				{
					difference = diff;
					next = keyframe.Value;
					nextKey = keyframe.Key;
				}
			}
			
			timeRemaining = elapsedTime - nextKey;

			var timeBetweenFrames = (nextKey - previousKey);
			var timeSinceLastKeyFrame = elapsedTime - previousKey;
			var lerpTime = (float) ((1f / timeBetweenFrames) * timeSinceLastKeyFrame);

			Vector3 previousVector = Evaluate(runtime, previous, false, currentValue);
			Vector3 nextVector = Evaluate(runtime, next, true, currentValue);

			
			return Vector3.Lerp(previousVector, nextVector, lerpTime);
		}
	}
}