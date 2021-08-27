using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Utils.Threading
{
	public class TaskStateUpdatedEventArgs : EventArgs
	{
		public TaskState PreviousState { get; }
		public TaskState NewState { get; }

		public TaskStateUpdatedEventArgs(TaskState previousState, TaskState newState)
		{
			PreviousState = previousState;
			NewState = newState;
		}
	}

	public class TaskCreatedEventArgs : TaskEventArgs
	{
		/// <inheritdoc />
		public TaskCreatedEventArgs(ManagedTask task) : base(task)
		{
			
		}
	}
	
	public class TaskFinishedEventArgs : TaskEventArgs
	{
		public TimeSpan ExecutionTime { get; }
		public TimeSpan TimeTillExecution { get; }
		
		/// <inheritdoc />
		public TaskFinishedEventArgs(ManagedTask task, TimeSpan executionTime, TimeSpan timeTillExecution) : base(task)
		{
			ExecutionTime = executionTime;
			TimeTillExecution = timeTillExecution;
		}
	}

	public class TaskEventArgs : EventArgs
	{
		public ManagedTask Task { get; }

		protected TaskEventArgs(ManagedTask task)
		{
			Task = task;
		}
	}
	
	public class ManagedTaskManager : GameComponent
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ManagedTaskManager));

		public EventHandler<TaskCreatedEventArgs> TaskCreated;
		public EventHandler<TaskFinishedEventArgs> TaskFinished;

		public int Pending => _queue.Count;
		public float AverageExecutionTime => _executionTimeMovingAverage.Average;
		public float AverageTimeTillExecution => _timeTillExecutionMovingAverage.Average;
		
		private ConcurrentQueue<ManagedTask> _queue = new ConcurrentQueue<ManagedTask>();
		private Alex _alex;

		private MovingAverage _executionTimeMovingAverage = new MovingAverage();
		private MovingAverage _timeTillExecutionMovingAverage = new MovingAverage();

		private bool _skipFrames = true;
		public ManagedTaskManager(Alex game) : base(game)
		{
			_alex = game;
			game.Options.AlexOptions.MiscelaneousOptions.SkipFrames.Bind(ListenerDelegate);
			_skipFrames = game.Options.AlexOptions.MiscelaneousOptions.SkipFrames.Value;
		}

		private void ListenerDelegate(bool oldvalue, bool newvalue)
		{
			_skipFrames = newvalue;
		}

		private uint _taskId = 0;
		private uint GetTaskId()
		{
			Interlocked.CompareExchange(ref _taskId, 0, uint.MaxValue);
			return Interlocked.Increment(ref _taskId);
		}

		private int _frameSkip = 0;
		/// <inheritdoc />
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (_skipFrames && _frameSkip > 0)
			{
				_frameSkip--;
				return;
			}

			if (_alex.FpsMonitor.IsRunningSlow)
				return;

			//var avgFrameTime = _alex.FpsMonitor.AverageFrameTime;

			//if (avgFrameTime <= 1f)
			//	avgFrameTime = _alex.FpsMonitor.AverageFrameTime;
			
			Stopwatch sw = Stopwatch.StartNew();

			//while (sw.Elapsed.TotalMilliseconds < avgFrameTime && !_queue.IsEmpty && _queue.TryDequeue(out var a))
			if (_queue.TryDequeue(out var a) && !a.IsCancelled)
			{
				var beforeRun = sw.Elapsed;

				TimeSpan timeTillExecution = a.TimeSinceCreation;
				try
				{
					a.Execute();
					_timeTillExecutionMovingAverage.ComputeAverage((float) timeTillExecution.TotalMilliseconds);
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Exception while executing enqueued task");
				}

				var afterRun = sw.Elapsed;
				var executionTime = (afterRun - beforeRun);
				_executionTimeMovingAverage.ComputeAverage((float) executionTime.TotalMilliseconds);

				TaskFinished?.Invoke(this, new TaskFinishedEventArgs(a, executionTime, timeTillExecution));

				if (_skipFrames)
				{
					var avgFrameTime = _alex.FpsMonitor.AverageFrameTime;

					if (executionTime.TotalMilliseconds > avgFrameTime)
					{
						var framesToSkip = (int)Math.Ceiling(executionTime.TotalMilliseconds / avgFrameTime);

						_frameSkip += framesToSkip;

						Log.Debug(
							$"Task execution time exceeds frametime by {(executionTime.TotalMilliseconds - avgFrameTime):F2}ms skipping {framesToSkip} frames (Tag={(a.Tag ?? "null")})");
					}
				}
			}
			//var elapsed = (float)sw.Elapsed.TotalMilliseconds;

			//if (elapsed > avgFrameTime)
			//	_frameSkip = (int)MathF.Ceiling(elapsed / avgFrameTime);
		}

		private void Enqueue(ManagedTask task)
		{
			TaskCreated?.Invoke(this, new TaskCreatedEventArgs(task));
			
			task.Enqueued();
			_queue.Enqueue(task);
		}
		
		public ManagedTask Enqueue(Action action)
		{
			ManagedTask task = new ManagedTask(GetTaskId(), action);
			Enqueue(task);

			return task;
		}

		public ManagedTask Enqueue(Action action, Action<ManagedTask> setupAction)
		{
			ManagedTask task = new ManagedTask(GetTaskId(), action);
			setupAction?.Invoke(task);
			Enqueue(task);

			return task;
		}
		
		public ManagedTask Enqueue(Action<object> action, object state)
		{
			ManagedTask task = new ManagedTask(GetTaskId(), action, state);
			Enqueue(task);

			return task;
		}

		public ManagedTask Enqueue(Action<ManagedTask, object> action, object state)
		{
			ManagedTask task = new ManagedTask(GetTaskId(), action, state);
			Enqueue(task);

			return task;
		}

		public ManagedTask Enqueue(Action<ManagedTask, object> action, object state, Action<ManagedTask> setupAction)
		{
			ManagedTask task = new ManagedTask(GetTaskId(), action, state);
			setupAction?.Invoke(task);
			Enqueue(task);

			return task;
		}
	}
}