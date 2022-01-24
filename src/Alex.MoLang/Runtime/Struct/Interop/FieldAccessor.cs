using System.Reflection;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime.Struct
{
	public class FieldAccessor : ValueAccessor
	{
		private FieldInfo _propertyInfo;

		public FieldAccessor(FieldInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
		}

		/// <inheritdoc />
		public override IMoValue Get(object instance)
		{
			var value = _propertyInfo.GetValue(instance);

			return value is IMoValue moValue ? moValue : MoValue.FromObject(value);
		}

		/// <inheritdoc />
		public override void Set(object instance, IMoValue value)
		{
			_propertyInfo.SetValue(instance, value);
		}
	}
}