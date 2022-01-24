using System;

namespace Alex.MoLang.Runtime.Value
{
	public class DoubleValue : IMoValue<double>
	{
		/// <inheritdoc />
		object IMoValue.Value => Value;

		/// <inheritdoc />
		public bool Equals(IMoValue b)
		{
			if (Value == b.AsDouble())
				return true;

			return false;
		}

		/// <inheritdoc />
		public double Value { get; set; }

		public DoubleValue(object value)
		{
			if (value is bool boolean)
			{
				Value = boolean ? 1.0 : 0.0;
			}
			else if (value is double dbl)
			{
				Value = dbl;
			}
			else if (value is float flt)
			{
				Value = flt;
			}
			else if (value is int integer)
			{
				Value = integer;
			}
			else
			{
				throw new NotSupportedException($"Cannot convert {value.GetType().FullName} to double");
			}
		}

		public DoubleValue(bool value)
		{
			Value = value ? 1d : 0d;
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

		/// <inheritdoc />
		public double AsDouble()
		{
			return Value;
		}

		/// <inheritdoc />
		public float AsFloat()
		{
			return (float)Value;
		}

		public static DoubleValue Zero { get; } = new DoubleValue(0d);
		public static DoubleValue One { get; } = new DoubleValue(1d);
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