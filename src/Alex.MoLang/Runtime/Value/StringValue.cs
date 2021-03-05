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
			if (Value == b.AsString())
				return true;

			return false;
		}
	}
}