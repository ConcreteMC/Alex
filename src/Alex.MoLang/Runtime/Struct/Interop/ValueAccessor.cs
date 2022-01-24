using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime.Struct
{
	public abstract class ValueAccessor
	{
		public abstract IMoValue Get(object instance);

		public abstract void Set(object instance, IMoValue value);
	}
}