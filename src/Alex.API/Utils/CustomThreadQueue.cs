using System;
using System.Collections.Generic;
using System.Threading;

namespace Alex.API.Utils
{
    public class CustomThreadQueue : IDisposable
    {
        private readonly LinkedList<Thread> _workers; // queue of worker threads ready to process actions
        private readonly LinkedList<Action> _tasks = new LinkedList<Action>(); // actions to be processed by worker threads
        private bool _disallowAdd; // set to true when disposing queue but there are still tasks pending
        private bool _disposed; // set to true when disposing queue and no more tasks are pending

        private long _runningTasks = 0;
        public int Running => (int)Interlocked.Read(ref _runningTasks);

        public CustomThreadQueue(int poolSize)
        {
            _workers = new LinkedList<Thread>();

            for (int i = 0; i < poolSize; i++)
            {
                var worker = new Thread(this.Worker) { Name = $"Worker: {i}" };
                worker.Start();
                this._workers.AddLast(worker);
            }
        }

        public void QueueStart(Action task)
        {
	        lock (this._tasks)
	        {
		        if (this._disallowAdd) { throw new InvalidOperationException("This Pool instance is in the process of being disposed, can't add anymore"); }
		        if (this._disposed) { throw new ObjectDisposedException("This Pool instance has already been disposed"); }
		        this._tasks.AddFirst(task);
		        Monitor.PulseAll(this._tasks); // pulse because tasks count changed
	        }
        }

        public void QueueTask(Action task)
        {
            lock (this._tasks)
            {
                if (this._disallowAdd) { throw new InvalidOperationException("This Pool instance is in the process of being disposed, can't add anymore"); }
                if (this._disposed) { throw new ObjectDisposedException("This Pool instance has already been disposed"); }
                this._tasks.AddLast(task);
                Monitor.PulseAll(this._tasks); // pulse because tasks count changed
            }
        }

        private void Worker()
        {
            Action task = null;
            while (true) // loop until threadpool is disposed
            {
                lock (this._tasks) // finding a task needs to be atomic
                {
                    while (true) // wait for our turn in _workers queue and an available task
                    {
                        if (this._disposed)
                        {
                            return;
                        }
                        if (null != this._workers.First && object.ReferenceEquals(Thread.CurrentThread, this._workers.First.Value) && this._tasks.Count > 0) // we can only claim a task if its our turn (this worker thread is the first entry in _worker queue) and there is a task available
                        {
                            task = this._tasks.First.Value;
                            this._tasks.RemoveFirst();
                            this._workers.RemoveFirst();
                            Monitor.PulseAll(this._tasks); // pulse because current (First) worker changed (so that next available sleeping worker will pick up its task)
                            break; // we found a task to process, break out from the above 'while (true)' loop
                        }
                        Monitor.Wait(this._tasks); // go to sleep, either not our turn or no task to process
                    }
                }

                Interlocked.Increment(ref _runningTasks);
                task(); // process the found task
                Interlocked.Decrement(ref _runningTasks);
                lock (this._tasks)
                {
                    this._workers.AddLast(Thread.CurrentThread);
                }
                task = null;
            }
        }

        public void Dispose()
        {
            var waitForThreads = false;
            lock (this._tasks)
            {
                if (!this._disposed)
                {
                    GC.SuppressFinalize(this);

                    this._disallowAdd = true; // wait for all tasks to finish processing while not allowing any more new tasks
                    while (this._tasks.Count > 0)
                    {
                        Monitor.Wait(this._tasks);
                    }

                    this._disposed = true;
                    Monitor.PulseAll(this._tasks); // wake all workers (none of them will be active at this point; disposed flag will cause then to finish so that we can join them)
                    waitForThreads = true;
                }
            }
            if (waitForThreads)
            {
                foreach (var worker in this._workers)
                {
                    worker.Join();
                }
            }
        }
    }
}
