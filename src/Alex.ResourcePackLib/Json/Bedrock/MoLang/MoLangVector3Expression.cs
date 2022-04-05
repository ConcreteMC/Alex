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
	/// <summary>
	///		Holds the actual keyframe data for a channel.
	///		(Rotation, Position, Scale etc)
	/// </summary>
	[JsonConverter(typeof(MoVec3Converter))]
	public class MoLangVector3Expression
	{
		private IExpression _x, _y, _z;

		public MoLangVector3Expression(IExpression[] values)
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

		private LinkedList<KeyValuePair<double, AnimationChannelData>> _keyFrames;

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

			_keyFrames = new LinkedList<KeyValuePair<double, AnimationChannelData>>();

			foreach (var keyframe in newKeyFrames.OrderBy(x => x.Key))
			{
				_keyFrames.AddLast(keyframe);
			}
		}

		private IVector3 Evaluate(MoLangRuntime runtime,
			IExpression xExpressions,
			IExpression yExpressions,
			IExpression zExpressions,
			IVector3 currentValue)
		{
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.X);
			IMoValue x = runtime.Execute(xExpressions);
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.Y);
			IMoValue y = runtime.Execute(yExpressions);
			runtime.Environment.ThisVariable = new DoubleValue(currentValue.Z);
			IMoValue z = runtime.Execute(zExpressions);

			return VectorUtils.VectorFactory.Vector3(x.AsFloat(), y.AsFloat(), z.AsFloat());
		}

		private IVector3 Evaluate(MoLangRuntime runtime, IExpression[] expressions, IVector3 currentValue)
		{
			if (expressions == null) return currentValue;

			if (expressions.Length == 3)
			{
				return Evaluate(runtime, expressions[0], expressions[1], expressions[2], currentValue);
			}

			return Evaluate(runtime, expressions[0], expressions[0], expressions[0], currentValue);
		}

		private IVector3 Evaluate(MoLangRuntime runtime, AnimationChannelData complex, bool isPre, IVector3 currentValue)
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

		public IVector3 Evaluate(MoLangRuntime runtime,
			IVector3 currentValue,
			double? animationLength = null,
			double elapsedTime = 0d,
			bool isRotational = false)
		{
			if (_keyFrames == null || !animationLength.HasValue) return Evaluate(runtime, _x, _y, _z, currentValue);


			KeyValuePair<double, AnimationChannelData>? previous = default;
			KeyValuePair<double, AnimationChannelData>? next = default;

			var f = _keyFrames.FirstOrDefault(x => (x.Key - elapsedTime) > 0d);
			var element = _keyFrames.Find(f) ?? _keyFrames.First;

			if (element == _keyFrames.First)
			{
				previous = _keyFrames.Last.Value;
			}
			else
			{
				previous = element.Previous.Value;
			}

			next = element.Value;

			var nextTimeStamp = next.Value.Key;
			var nextChannelData = next.Value.Value;

			var previousTimeStamp = previous.Value.Key;
			var previousChannelData = previous.Value.Value;

			if (element == _keyFrames.First) //If this is the first frame, we need to adjust for that.
			{
				previousTimeStamp = (animationLength.Value - previous.Value.Key);
			}

			var timeBetweenFrames = (nextTimeStamp - previousTimeStamp);
			var timeSinceLastKeyFrame = elapsedTime - previousTimeStamp;

			var lerpTime = (float)(timeSinceLastKeyFrame / timeBetweenFrames);

			IVector3 previousVector = Evaluate(runtime, previousChannelData, false, currentValue);
			IVector3 nextVector = Evaluate(runtime, nextChannelData, true, currentValue);

			if (isRotational)
				return VectorUtils.LerpVector3Degrees(previousVector, nextVector, lerpTime);

			return VectorUtils.Lerp(previousVector, nextVector, lerpTime);
		}
	}
}