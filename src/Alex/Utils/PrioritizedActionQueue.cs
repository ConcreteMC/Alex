using System;
using System.Collections.Generic;
using System.Threading;
using MiNET.Utils;

namespace Alex.Utils
{
    public class PrioritizedActionQueue
    {
        private DedicatedThreadPool _threadPool;
        private PriorityQueue<EnqueuedTask> Queue { get; }
        
        private int MaxConcurrent { get; }
        private long _workerCount = 0;
        
        public PrioritizedActionQueue(DedicatedThreadPool threadPool, int concurrentTasks)
        {
            MaxConcurrent = concurrentTasks;
            _threadPool = threadPool;
            Queue = new PriorityQueue<EnqueuedTask>();
        }

        private ReaderWriterLockSlim Lock { get; } = new ReaderWriterLockSlim();

        public EnqueuedTask Enqueue(double priority, Action action)
        {
            Lock.EnterWriteLock();
            try
            {
                EnqueuedTask task = new EnqueuedTask(this, priority, action, Guid.NewGuid());

                Queue.Enqueue(task);

                if (Interlocked.Read(ref _workerCount) < MaxConcurrent)
                {
                    Interlocked.Increment(ref _workerCount);
                    _threadPool.QueueUserWorkItem(Worker);
                }

                return task;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        private object _queueLock = new object();

        private bool TryDequeue(out EnqueuedTask task)
        {
            Lock.EnterWriteLock();
            try
            {
                if (Queue.Count() != 0)
                {
                    task = Queue.Dequeue();
                    return true;
                }

                task = null;
                return false;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
        
        private void Worker()
        {
            try
            {
                while (TryDequeue(out EnqueuedTask task))
                {
                    if (task.Canceled)
                        continue;
                    
                    task.Action?.Invoke();
                }
            }
            finally
            {
                Interlocked.Decrement(ref _workerCount);
            }
        }

        internal void PriorityUpdated(Guid taskId, double newPriority)
        {
            
        }

        internal void CancelTask(Guid taskId)
        {
            
        }
    }

    public class EnqueuedTask : IComparable<EnqueuedTask>
    {
        private double _priority = 0d;//TaskPriority.Normal;

        public double Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                var oldValue = _priority;
                _priority = value;
                
                Parent.PriorityUpdated(TaskIdentifier, value);
            }
        }
        
        public Guid TaskIdentifier { get; }

        internal Action Action { get; }
        private PrioritizedActionQueue Parent { get; }
        public EnqueuedTask(PrioritizedActionQueue parent, double priority, Action action, Guid taskIdentifier)
        {
            Parent = parent;
            Priority = priority;
            Action = action;
            TaskIdentifier = taskIdentifier;
        }

        internal bool Canceled { get; set; } = false;

        public void Cancel()
        {
            Canceled = true;
            Parent.CancelTask(TaskIdentifier);
        }

        public int CompareTo(EnqueuedTask other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return _priority.CompareTo(other._priority);
            //if (priorityComparison != 0) return priorityComparison;
           // return TaskIdentifier.CompareTo(other.TaskIdentifier);
        }
    }
}