using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Alex.Common.Utils;
using Alex.Worlds.Chunks;
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
		private MovingAverage _executionTimeMovingAverage = new MovingAverage();
		private MovingAverage _timeTillExecutionMovingAverage = new MovingAverage();

		private bool _skipFrames = true;
		public ManagedTaskManager(Alex game) : base(game)
		{
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

		private int _tasksEnqueued = 0;
		private int _tasksExecuted = 0;
		private int _frameSkip = 0;
		private int _framesSkipped = 0;
		private double _executionTimeTotal = 0;
		private double _accumulator = 0d;

		/// <inheritdoc />
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			_accumulator += gameTime.ElapsedGameTime.TotalMilliseconds;

			if (_accumulator >= 1000)
			{
				var enqueued = Interlocked.Exchange(ref _tasksEnqueued, 0);
				var executed = Interlocked.Exchange(ref _tasksExecuted, 0);
				var framesSkipped = Interlocked.Exchange(ref _framesSkipped, 0);
				var executionTime = Interlocked.Exchange(ref _executionTimeTotal, 0d);
				var bufferUploads = Interlocked.Exchange(ref ChunkData.BufferUploads, 0);
				var bufferCreations = Interlocked.Exchange(ref ChunkData.BufferCreations, 0);

			//	Log.Info(
				//	$"Tasks enqueued (#/s){enqueued}, Tasks executed (#/s){executed}, Frames Skipped (#/s){framesSkipped}, Time (ms) {executionTime:F2}, BufferUpdates (#/s) {bufferUploads}");

				_accumulator = 0;
			}

			if (_skipFrames && _frameSkip > 0)
			{
				Interlocked.Increment(ref _framesSkipped);
				_frameSkip--;

				return;
			}

			Stopwatch sw = Stopwatch.StartNew();
			if (_queue.TryDequeue(out var a))
			{
				if (a.IsCancelled)
					return;
				
				var beforeRun = sw.Elapsed;

				TimeSpan timeTillExecution = a.TimeSinceCreation;

				try
				{
					a.Execute();
					_timeTillExecutionMovingAverage.ComputeAverage((float)timeTillExecution.TotalMilliseconds);
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Exception while executing enqueued task");
				}

				var afterRun = sw.Elapsed;
				var executionTime = (afterRun - beforeRun);
				_executionTimeMovingAverage.ComputeAverage((float)executionTime.TotalMilliseconds);

				TaskFinished?.Invoke(this, new TaskFinishedEventArgs(a, executionTime, timeTillExecution));

				Interlocked.Increment(ref _tasksExecuted);

				if (_skipFrames)
				{
					if (executionTime.TotalMilliseconds > gameTime.ElapsedGameTime.TotalMilliseconds)
					{
						var framesToSkip = (int)Math.Ceiling(executionTime.TotalMilliseconds / gameTime.ElapsedGameTime.TotalMilliseconds);

						_frameSkip += framesToSkip;

						Log.Debug(
							$"Task execution time exceeds frametime by {(executionTime.TotalMilliseconds - gameTime.ElapsedGameTime.TotalMilliseconds):F2}ms skipping {framesToSkip} frames (Tag={(a.Tag ?? "null")})");
					}
				}
			}


			var elapsed = sw.Elapsed.TotalMilliseconds;
			_executionTimeTotal += elapsed;
		}

		private void Enqueue(ManagedTask task)
		{
			TaskCreated?.Invoke(this, new TaskCreatedEventArgs(task));

			Interlocked.Increment(ref _tasksEnqueued);
			task.Enqueued();
			//task.Execute();
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