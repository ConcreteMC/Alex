using System;

namespace Alex.Utils.Tasks
{
	public class ManagedTask
	{
		public TaskState State { get; private set; } = TaskState.Created;
		public bool IsCancelled => State == TaskState.Cancelled;
		
		private readonly Action _action;
		public ManagedTask(Action action)
		{
			_action = action;
		}

		public object Data { get; set; } = null;
		private readonly Action<object> _parameterizedTask;
		private DateTime _enqueueTime = DateTime.UtcNow;
		public ManagedTask(Action<object> action, object state)
		{
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

		public TimeSpan Execute()
		{
			if (State != TaskState.Enqueued)
				return TimeSpan.Zero;
			
			State = TaskState.Running;
			
			try
			{
				if (_action != null)
				{
					_action?.Invoke();
				}else if (_parameterizedTask != null)
				{
					_parameterizedTask?.Invoke(Data);
				}
			}
			finally
			{
				State = TaskState.Finished;
				Data = null;
			}

			return DateTime.UtcNow - _enqueueTime;
		}

		public void Cancel()
		{
			if (State == TaskState.Enqueued)
				State = TaskState.Cancelled;
		}
	}
}