using System;
using System.Collections.Generic;

namespace Alex.Common.Utils
{
	public class MovingAverage
	{
		private readonly Queue<float> _samples = new Queue<float>();
		private readonly int _windowSize = 128;
		private float _sampleAccumulator;
		public float Average { get; private set; }

		public float Minimum { get; private set; }
		public float Maximum { get; private set; }

		public MovingAverage() : this(128) { }

		public MovingAverage(int windowSize)
		{
			_windowSize = windowSize;
		}

		/// <summary>
		/// Computes a new windowed average each time a new sample arrives
		/// </summary>
		/// <param name="newSample"></param>
		public float ComputeAverage(float newSample)
		{
			_sampleAccumulator += newSample;
			_samples.Enqueue(newSample);

			if (_samples.Count > _windowSize)
			{
				_sampleAccumulator -= _samples.Dequeue();
			}

			Average = _sampleAccumulator / _samples.Count;
			Minimum = Math.Min(Minimum, newSample);
			Maximum = Math.Max(Maximum, newSample);

			return Average;
		}
	}
}