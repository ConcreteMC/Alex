namespace Alex.MoLang.Runtime.Value
{
	public class DoubleValue : IMoValue<double>
	{
		/// <inheritdoc />
		object IMoValue.Value => Value;

		/// <inheritdoc />
		public double Value { get; }

		public DoubleValue(object value) {
			if (value is bool) {
				Value = (bool) value ? 1.0 : 0.0;
			} else if (value is double) {
				Value = (double) value;
			} else {
				Value = 1.0;
			}
		}

		public static DoubleValue Zero => new DoubleValue(0);
		public static DoubleValue One => new DoubleValue(1);
	}
}