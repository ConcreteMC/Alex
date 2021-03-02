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
		
		public DoubleValue(double value)
		{
			Value = value;
		}

		public static DoubleValue Zero => new DoubleValue(0);
		public static DoubleValue One => new DoubleValue(1);
	}
	
	/*public class FloatValue : IMoValue<float>
	{
		/// <inheritdoc />
		object IMoValue.Value => Value;

		/// <inheritdoc />
		public float Value { get; }

		public FloatValue(object value) {
			if (value is bool) {
				Value = (bool) value ? 1.0f : 0.0f;
			} else if (value is float) {
				Value = (float) value;
			} else {
				Value = 1.0f;
			}
		}
		
		public FloatValue(float value)
		{
			Value = value;
		}

		public static FloatValue Zero => new FloatValue(0);
		public static FloatValue One => new FloatValue(1);
	}*/
}