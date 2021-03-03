namespace Alex.MoLang.Runtime.Value

{
	public interface IMoValue<T> : IMoValue
	{
		new T Value { get; }
	}

	public interface IMoValue
	{
		object Value { get; }


		string AsString() => Value.ToString();

		double AsDouble() => Value is double db ? db : 0d;
		float AsFloat() => Value is float flt ? flt : (float)AsDouble();
		bool AsBool() => Value is bool b ? b : AsDouble() > 0;
	}

	public static class MoValue
	{
		public static IMoValue FromObject(object value)
		{
			if (value is IMoValue) {
				return (IMoValue) value;
			}
			
			return new DoubleValue(value);
		}
	}
}