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
		public float MaxFrameTime => _movingAverage.Maximum;
		public float MinFrameTime => _movingAverage.Minimum;

		/// <summary>
		///		Returns true if the average framerate over the last few frames deviates too much from the games average FPS.
		/// </summary>
		public bool IsRunningSlow { get; private set; } = false;
		
		public           TimeSpan  SampleRate           { get; set; }
		private readonly Stopwatch _sw;
		private          int       _frames;

		private Stopwatch _frameSw;
		private MovingAverage _movingAverage;
		private MovingAverage _lastFewFrames;
		private Alex _alex;
		public FpsMonitor(Alex game) : base(game)
		{
			_alex = game;
			_movingAverage = new MovingAverage(256);
			_lastFewFrames = new MovingAverage(8);
			
			this.SampleRate = TimeSpan.FromSeconds(1);
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
            if (_sw.Elapsed > SampleRate)
			{
				this.Value = (float)(_frames / _sw.Elapsed.TotalSeconds);
				//this.AverageFrameTime = _sw.Elapsed / _frames;
				this._sw.Reset();
				this._sw.Start();
				this._frames = 0;
			}

            LastFrameTime = (float) _frameSw.Elapsed.TotalMilliseconds;
            
            var currentAverage = _lastFewFrames.ComputeAverage(LastFrameTime);
            var smoothAverage = _movingAverage.ComputeAverage(LastFrameTime);

            //If the frametime over the last few frames is higher than the average, the game is considered slow.
            IsRunningSlow = (smoothAverage / currentAverage)
                            < _alex.Options.AlexOptions.MiscelaneousOptions.AntiLagModifier;// 0.75f;
			_frameSw.Restart();
		}
	}
}
