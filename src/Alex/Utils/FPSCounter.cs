using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
	public class FpsMonitor : DrawableGameComponent
	{
		public float    Value            { get; private set; }
		public TimeSpan AverageFrameTime { get; private set; }
		public TimeSpan LastFrameTime    { get; private set; }

		public           TimeSpan  Sample           { get; set; }
		private readonly Stopwatch _sw;
		private          int       _frames;

		private Stopwatch _frameSw;
		public FpsMonitor(Game game) : base(game)
		{
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
				this.AverageFrameTime = _sw.Elapsed / _frames;
				this._sw.Reset();
				this._sw.Start();
				this._frames = 0;
			}

            LastFrameTime = _frameSw.Elapsed;
            _frameSw.Restart();
		}
	}
}
