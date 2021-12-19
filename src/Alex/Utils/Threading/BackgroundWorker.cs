using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Alex.Utils.Threading
{
	public class BackgroundWorker : IDisposable
	{
		private CancellationTokenSource _cancellationTokenSource;
		private BlockingCollection<Action> _workerQueue             = new BlockingCollection<Action>();
		
		public int MaxThreads { get; set; }
		
		//private Thread _workerThread;
		public BackgroundWorker(CancellationToken cancellationToken, int threads = 1)
		{
			MaxThreads = threads;
			_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			cancellationToken = _cancellationTokenSource.Token;
			
			var task = new Task(
				() =>
				{
					Thread.CurrentThread.Name = $"BackgroundWorker Thread";

					while (!_workerQueue.IsCompleted && _workerQueue.TryTake(out var action, -1, cancellationToken))
					{
						action?.Invoke();
					}
				}, cancellationToken, TaskCreationOptions.LongRunning);
			
			task.Start();
		}

		public void Enqueue(Action action)
		{
			_workerQueue.Add(action);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_workerQueue?.CompleteAdding();
			if (!_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Cancel();
			}
			
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;
			
			_workerQueue?.Dispose();
			_workerQueue = null;
		}
	}
}