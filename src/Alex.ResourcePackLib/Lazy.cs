using System;

namespace Alex.ResourcePackLib
{
	public class LazyO<T> where T : class
	{
		private Func<T> _valueGenerator;
		private T       _value = null;
 		public LazyO(Func<T> valueGenerator)
        {
	        _valueGenerator = valueGenerator;
        }

        public T Value
        {
	        get
	        {
		        if (_value == null)
		        {
			        _value = _valueGenerator();
		        }

		        return _value;
	        }
        }
		public static implicit operator T(LazyO<T> val)
		{
			return val.Value;
		}
	}
}