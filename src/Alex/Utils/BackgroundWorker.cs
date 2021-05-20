using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Alex.Utils
{
	public class BackgroundWorker : IDisposable
	{
		private CancellationTokenSource _cancellationTokenSource;
		private ConcurrentQueue<Action> _workerQueue             = new ConcurrentQueue<Action>();
		private ManualResetEvent        _manualResetEvent        = new ManualResetEvent(false);

		public int MaxThreads { get; set; }
		
		//private Thread _workerThread;
		public BackgroundWorker(CancellationToken cancellationToken, int threads = 1)
		{
			MaxThreads = threads;
			_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			
			var task = new Task(
				() =>
				{
					Thread.CurrentThread.Name = $"BackgroundWorker Thread";
					//_workerThread = Thread.CurrentThread;
					while (!_cancellationTokenSource.IsCancellationRequested)
					{
						if (_manualResetEvent.WaitOne(50))
						{
							if (_cancellationTokenSource.IsCancellationRequested)
								break;

							while (_workerQueue.TryDequeue(out var action))
							{
								action?.Invoke();

								if (_cancellationTokenSource.IsCancellationRequested)
									break;
							}

							_manualResetEvent.Reset();
						}
					
						if (_cancellationTokenSource.IsCancellationRequested)
							break;
					}
				}, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
			
			task.Start();
		}

		public void Enqueue(Action action)
		{
			_workerQueue.Enqueue(action);
			_manualResetEvent.Set();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			
			_manualResetEvent?.Dispose();
		}
	}
}