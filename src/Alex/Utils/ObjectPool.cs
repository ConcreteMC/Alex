using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Alex.Utils
{
	public class ObjectPool
	{
		private readonly ConcurrentBag<object> _objects;
		private readonly Func<object>          _objectGenerator;

		private AutoResetEvent _resetEvent = new AutoResetEvent(true);
		private int _poolSize = 0;
		public ObjectPool(int poolSize, Func<object> objectGenerator)
		{
			_objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
			_objects = new ConcurrentBag<object>();

			for (int i = 0; i < poolSize; i++)
			{
				_objects.Add(_objectGenerator());
			}
		}

		public virtual object GetPooled()
		{
			object o;
			while (!_objects.TryTake(out o))
			{
				_resetEvent.WaitOne();
			}

			return o;
		}

		public virtual void ReturnToPool(object item)
		{
			_objects.Add(item);
			_resetEvent.Set();
		}
	}
	
	public class ObjectPool<T> : ObjectPool where T : PooledObject
	{
		public ObjectPool(int poolSize, Func<T> objectGenerator) : base(poolSize, objectGenerator)
		{

		}

		public T Get()
		{
			var result = (T) base.GetPooled();
			result.Parent = this;

			return result;
		}
	}

	public abstract class PooledObject : IDisposable
	{
		public ObjectPool Parent { get; set; }
		
		protected abstract void Reset();

		/// <inheritdoc />
		public void Dispose()
		{
			Reset();
			
			Parent?.ReturnToPool(this);
			Parent = null;
		}
	}
}