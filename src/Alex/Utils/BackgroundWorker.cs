using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Alex.Utils
{
	public class BackgroundWorker : IDisposable
	{
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private ConcurrentQueue<Action> _workerQueue             = new ConcurrentQueue<Action>();
		private ManualResetEvent        _manualResetEvent        = new ManualResetEvent(false);

		private Thread _workerThread;
		public BackgroundWorker(int threads = 1)
		{
			ThreadPool.QueueUserWorkItem((o) =>
			{
				_workerThread = Thread.CurrentThread;

				while (!_cancellationTokenSource.IsCancellationRequested)
				{
					_manualResetEvent.WaitOne();
					
					while (_workerQueue.TryDequeue(out var action))
					{
						action?.Invoke();
					}

					_manualResetEvent.Reset();
				}
			});
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