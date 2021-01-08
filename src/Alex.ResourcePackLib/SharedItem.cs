using System;
using System.Threading;

namespace Alex.ResourcePackLib
{
	public class SharedItem<T> : IDisposable where T : IDisposable
	{
		private T       _value;
		private Func<T> _valueGenerator;
		private long    _referenceHolders = 0;
		public SharedItem(Func<T> valueGenerator)
		{
			_valueGenerator = valueGenerator;
		}

		

		private object _lock = new object();

		/// <inheritdoc />
		public void Dispose()
		{
			lock (_lock)
			{
				if (Interlocked.Decrement(ref _referenceHolders) == 0)
				{
					_value.Dispose();
				}
			}
		}
	}
}