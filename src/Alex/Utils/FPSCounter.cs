using System;
using System.Diagnostics;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
	public class FpsMonitor : DrawableGameComponent
	{
		public float    Value            { get; private set; }
		public float LastFrameTime    { get; private set; }
		public float AverageFrameTime => _movingAverage.Average;
		public bool IsRunningSlow => _movingAverage2.Average < _movingAverage.Average;
		
		public           TimeSpan  Sample           { get; set; }
		private readonly Stopwatch _sw;
		private          int       _frames;

		private Stopwatch _frameSw;
		private MovingAverage _movingAverage;
		private MovingAverage _movingAverage2;
		public FpsMonitor(Game game) : base(game)
		{
			_movingAverage = new MovingAverage();
			_movingAverage2 = new MovingAverage(5);
			
			this.Sample = TimeSpan.FromSeconds(1);
			this.Value = 0;
			this._frames = 0;
			this._sw = Stopwatch.StartNew();
			this._frameSw = Stopwatch.StartNew();
		}

		/// <inheritdoc />
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
			this._frames++;
            if (_sw.Elapsed > Sample)
			{
				this.Value = (float)(_frames / _sw.Elapsed.TotalSeconds);
				//this.AverageFrameTime = _sw.Elapsed / _frames;
				this._sw.Reset();
				this._sw.Start();
				this._frames = 0;
			}

            LastFrameTime = (float) _frameSw.Elapsed.TotalMilliseconds;
            
			_movingAverage.ComputeAverage(LastFrameTime);
            _movingAverage2.ComputeAverage(LastFrameTime);
            
            _frameSw.Restart();
		}
	}
}
