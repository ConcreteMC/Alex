using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Utils.Tasks
{
	public class ManagedTaskManager : GameComponent
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ManagedTaskManager));

		public int Count => _queue.Count;
		public float AverageExecutionTime => _executionTimeMovingAverage.Average;
		public float AverageTimeTillExecution => _timeTillExecutionMovingAverage.Average;
		
		private ConcurrentQueue<ManagedTask> _queue = new ConcurrentQueue<ManagedTask>();
		private Alex _alex;

		private MovingAverage _executionTimeMovingAverage = new MovingAverage();
		private MovingAverage _timeTillExecutionMovingAverage = new MovingAverage();
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

				//TimeSpan timeTillExecution;
				try
				{
					var timeTillExecution = a.Execute();
					_timeTillExecutionMovingAverage.ComputeAverage((float) timeTillExecution.TotalMilliseconds);
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Exception while executing enqueued task");
				}

				var afterRun = sw.Elapsed.TotalMilliseconds;
				_executionTimeMovingAverage.ComputeAverage((float) (afterRun - beforeRun));
			}

			var elapsed = (float)sw.Elapsed.TotalMilliseconds;

			if (elapsed > avgFrameTime)
				_frameSkip = (int)MathF.Ceiling(elapsed / avgFrameTime);
		}

		public ManagedTask Enqueue(Action action)
		{
			ManagedTask task = new ManagedTask(action);
			task.Enqueued();
			_queue.Enqueue(task);

			return task;
		}
		
		public ManagedTask Enqueue(Action<object> action, object state)
		{
			ManagedTask task = new ManagedTask(action, state);
			task.Enqueued();
			_queue.Enqueue(task);

			return task;
		}
	}
}