using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Utils.Tasks
{
	public class ManagedTaskManager : GameComponent
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ManagedTaskManager));

		public int Count => _queue.Count;
		public float AverageExecutionTime => _movingAverage.Average;
		
		private ConcurrentQueue<ManagedTask> _queue = new ConcurrentQueue<ManagedTask>();
		private Alex _alex;

		private MovingAverage _movingAverage = new MovingAverage();
		public ManagedTaskManager(Alex game) : base(game)
		{
			_alex = game;
		}

		private int _frameSkip = 0;
		/// <inheritdoc />
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (_frameSkip > 0)
			{
				_frameSkip--;

				return;
			}

			if (_alex.FpsMonitor.IsRunningSlow)
				return;

			var avgFrameTime = _alex.FpsMonitor.AverageFrameTime;
			Stopwatch sw = Stopwatch.StartNew();
			while (sw.Elapsed.TotalMilliseconds < avgFrameTime && !_queue.IsEmpty && _queue.TryDequeue(out var a))
			{
				if (a.IsCancelled)
					continue;

				var beforeRun = sw.Elapsed.TotalMilliseconds;
				try
				{
					a.Execute();
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Exception while executing enqueued task");
				}

				var afterRun = sw.Elapsed.TotalMilliseconds;
				_movingAverage.ComputeAverage((float) (afterRun - beforeRun));
			}

			var elapsed = (float)sw.Elapsed.TotalMilliseconds;

			if (elapsed > avgFrameTime)
				_frameSkip = (int)MathF.Ceiling(elapsed / avgFrameTime);
		}

		public ManagedTask Enqueue(Action action)
		{
			ManagedTask task = new ManagedTask(action);
			_queue.Enqueue(task);

			return task;
		}
		
		public ManagedTask Enqueue(Action<object> action, object state)
		{
			ManagedTask task = new ManagedTask(action, state);
			_queue.Enqueue(task);

			return task;
		}
	}
}