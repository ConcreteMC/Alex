using System;

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
			} else if (value is float flt){
				Value = flt;
			}
			else
			{
				throw new NotSupportedException($"Cannot convert {value.GetType().FullName} to double");
			}
		}
		
		public DoubleValue(double value)
		{
			Value = value;
		}
		
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;

			return (obj is DoubleValue dv && dv.Value == Value);
			// ...the rest of the equality implementation
		}

		public static DoubleValue Zero => new DoubleValue(0d);
		public static DoubleValue One => new DoubleValue(1d);
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