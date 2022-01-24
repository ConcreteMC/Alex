namespace Alex.MoLang.Runtime.Value
{
	public class StringValue : IMoValue<string>
	{
		/// <inheritdoc />
		object IMoValue.Value => Value;

		/// <inheritdoc />
		public string Value { get; set; }

		public StringValue(string value)
		{
			Value = value;
		}

		/// <inheritdoc />
		public bool Equals(IMoValue b)
		{
			if (b is StringValue stringValue)
				return string.Equals(Value, stringValue.Value); // stringValue.Value.

			return false;
		}
	}
}