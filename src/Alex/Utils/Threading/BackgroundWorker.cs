using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Alex.Networking.Java.Packets.Play;
using NLog;

namespace Alex.Utils.Threading
{
	public class BackgroundWorker : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BackgroundWorker));
		private CancellationTokenSource _cancellationTokenSource;
		private BlockingCollection<Action> _workerQueue = null;

		public int MaxThreads { get; set; }

		//private Thread _workerThread;
		public BackgroundWorker(CancellationToken cancellationToken, int threads = 1)
		{
			MaxThreads = threads;


			Start();
		}

		private void HandleAction(Action obj)
		{
			try
			{
				obj?.Invoke();
			}
			catch (Exception ex)
			{
				Log.Warn(ex, "Exception in queued item!");
			}
		}

		public void Start()
		{
			if (_workerQueue != null)
				return;

			_cancellationTokenSource = new CancellationTokenSource();
			_workerQueue = new BlockingCollection<Action>();


			var task = new Thread(
				() =>
				{
					Thread.CurrentThread.Name = $"BackgroundWorker Thread";

					while (!_cancellationTokenSource.IsCancellationRequested && _workerQueue != null
					                                                         && !_workerQueue.IsCompleted)
					{
						if (_workerQueue.Count <= 0)
						{
							Thread.Yield();

							continue;
						}

						if (_workerQueue.TryTake(out var action, -1, _cancellationTokenSource.Token))
						{
							HandleAction(action);
						}
					}
				});

			task.Start();
		}

		public void Enqueue(Action action)
		{
			//_queue?.Post(action);
			_workerQueue?.Add(action);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			//return;
			if (_workerQueue == null)
				return;

			_workerQueue?.CompleteAdding();

			if (!_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Cancel();
			}

			_cancellationTokenSource?.Dispose();
			//	_cancellationTokenSource = null;

			_workerQueue?.Dispose();
			_workerQueue = null;
		}
	}
}