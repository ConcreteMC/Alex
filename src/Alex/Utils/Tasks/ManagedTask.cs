using System;

namespace Alex.Utils.Tasks
{
	public class ManagedTask
	{
		public TaskState State { get; private set; } = TaskState.Enqueued;
		public bool IsCancelled => State == TaskState.Cancelled;
		
		private readonly Action _action;
		public ManagedTask(Action action)
		{
			_action = action;
		}

		public object Data { get; set; } = null;
		private readonly Action<object> _parameterizedTask;
		public ManagedTask(Action<object> action, object state)
		{
			_parameterizedTask = action;
			Data = state;
		}

		public void Execute()
		{
			if (State != TaskState.Enqueued)
				return;
			
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
			}
		}

		public void Cancel()
		{
			if (State == TaskState.Enqueued)
				State = TaskState.Cancelled;
		}
	}
}