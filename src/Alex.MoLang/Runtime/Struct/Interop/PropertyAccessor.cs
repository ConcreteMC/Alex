using System.Reflection;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime.Struct
{
	public class PropertyAccessor : ValueAccessor
	{
		private PropertyInfo _propertyInfo;

		public PropertyAccessor(PropertyInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
		}

		/// <inheritdoc />
		public override IMoValue Get(object instance)
		{
			var value = _propertyInfo.GetValue(instance);

			return value is IMoValue moValue ? moValue : MoValue.FromObject(value);
			return (IMoValue)_propertyInfo.GetValue(instance);
		}

		/// <inheritdoc />
		public override void Set(object instance, IMoValue value)
		{
			if (!_propertyInfo.CanWrite)
				return;

			_propertyInfo.SetValue(instance, value);
		}
	}
}