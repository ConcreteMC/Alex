namespace Alex.MoLang.Runtime.Value

{
	public interface IMoValue<T> : IMoValue
	{
		T Value { get; }
	}

	public interface IMoValue
	{
		object Value { get; }


		string AsString() => Value.ToString();

		double AsDouble() => Value is double ? (double)Value : 1.0d;
		float AsFloat() => Value is float ? (float)Value : (float)AsDouble();
	}

	public static class MoValue
	{
		public static IMoValue FromObject(object value)
		{
			if (value is IMoValue) {
				return (IMoValue) value;
			}
			
			return new DoubleValue((double) value);
		}
	}
}