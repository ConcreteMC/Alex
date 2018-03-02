using System;
using System.Diagnostics;

namespace Alex.Utils
{
	public class FpsMonitor
	{
		public float Value { get; private set; }
		public TimeSpan Sample { get; set; }
		private readonly Stopwatch _sw;
		private int _frames;

		public FpsMonitor()
		{
			this.Sample = TimeSpan.FromSeconds(1);
			this.Value = 0;
			this._frames = 0;
			this._sw = Stopwatch.StartNew();
		}

		public void Update()
		{
            this._frames++;
            if (_sw.Elapsed > Sample)
			{
				this.Value = (float)(_frames / _sw.Elapsed.TotalSeconds);
				this._sw.Reset();
				this._sw.Start();
				this._frames = 0;
			}
		}
	}
}
