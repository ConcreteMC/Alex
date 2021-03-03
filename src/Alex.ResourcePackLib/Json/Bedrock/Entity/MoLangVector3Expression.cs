using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	/// <summary>
	///		Holds the actual keyframe data for a channel.
	///		(Rotation, Position, Scale etc)
	/// </summary>
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
}