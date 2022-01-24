using System;
using System.Collections.Concurrent;
using NLog;

namespace Alex.Networking.Java.Packets
{
	public class PacketPool<T> where T : Packet
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		private ConcurrentQueue<T> _objects;

		//private ConcurrentBag<T> _objects;
		private Func<T> _objectGenerator;
		public int Size => _objects.Count;

		public void FillPool(int count)
		{
			for (int i = 0; i < count; i++)
			{
				var item = _objectGenerator();
				_objects.Enqueue(item);
			}
		}

		public PacketPool(Func<T> objectGenerator)
		{
			if (objectGenerator == null) throw new ArgumentNullException("objectGenerator");
			//_objects = new ConcurrentBag<T>();
			_objects = new ConcurrentQueue<T>();
			_objectGenerator = objectGenerator;
		}

		public T GetObject()
		{
			if (_objects.TryDequeue(out T item)) return item;

			return _objectGenerator();
		}

		const long MaxPoolSize = 10000000;

		public void PutObject(T item)
		{
			//if (_objects.Count > MaxPoolSize)
			//{
			//	Log.Warn($"Pool for {typeof (T).Name} is full");
			//	return;
			//}

			_objects.Enqueue(item);
		}
	}
}