using System;
using System.Diagnostics;

namespace Alex.Utils.Threading
{
	public class ManagedTask
	{
		public EventHandler<TaskStateUpdatedEventArgs> StateChanged;

		public TaskState State
		{
			get => _state;
			private set
			{
				var previousState = _state;
				_state = value;
				
				StateChanged?.Invoke(this, new TaskStateUpdatedEventArgs(previousState, value));
			}
		}

		public bool IsCancelled => State == TaskState.Cancelled;
		public TimeSpan ExecutionTime => _executionStopwatch.Elapsed;
		public TimeSpan TimeSinceCreation => DateTime.UtcNow - _enqueueTime;
		
		public object Tag { get; set; } = null;
		public uint Identifier { get; } = 0;
		
		private Stopwatch _executionStopwatch = new Stopwatch();
		private readonly Action _action;
		public ManagedTask(uint identifier, Action action)
		{
			Identifier = identifier;
			_action = action;
		}

		public object Data { get; set; } = null;
		private readonly Action<ManagedTask, object> _parameterizedTask;
		private DateTime _enqueueTime = DateTime.UtcNow;
		private TaskState _state = TaskState.Created;

		public ManagedTask(uint identifier, Action<object> action, object state)
		{
			Identifier = identifier;
			_parameterizedTask = (t, s) => action(s);
			Data = state;
		}
		
		public ManagedTask(uint identifier, Action<ManagedTask, object> action, object state)
		{
			Identifier = identifier;
			_parameterizedTask = action;
			Data = state;
		}

		public void Enqueued()
		{
			if (State != TaskState.Created)
				return;

			State = TaskState.Enqueued;
			_enqueueTime = DateTime.UtcNow;
		}

		/// <summary>
		///		Executes the task
		/// </summary>
		/// <returns>The amount of time elapsed since the task was originally enqueued.</returns>
		public bool Execute()
		{
			if (State != TaskState.Enqueued)
				return false;
			
			State = TaskState.Running;
			_executionStopwatch.Start();
			
			try
			{
				if (_action != null)
				{
					_action?.Invoke();
				}else if (_parameterizedTask != null)
				{
					_parameterizedTask?.Invoke(this, Data);
				}
			}
			finally
			{
				_executionStopwatch.Stop();
				State = TaskState.Finished;
				Data = null;
			}

			return true;
		}

		public void Cancel()
		{
			if (State == TaskState.Enqueued)
				State = TaskState.Cancelled;
		}
	}
}