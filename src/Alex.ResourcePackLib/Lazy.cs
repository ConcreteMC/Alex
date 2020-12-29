using System;

namespace Alex.ResourcePackLib
{
	public class Lazy<T> where T : class
	{
		private Func<T> _valueGenerator;
		private T       _value = null;
 		public Lazy(Func<T> valueGenerator)
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
		public static implicit operator T(Lazy<T> val)
		{
			return val.Value;
		}
	}
}