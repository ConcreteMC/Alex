namespace Alex.MoLang.Runtime.Value
{
	public class StringValue : IMoValue<string>
	{
		private string _value;

		/// <inheritdoc />
		object IMoValue.Value => Value;

		/// <inheritdoc />
		public string Value { get; }

		public StringValue(string value)
		{
			Value = value;
		}
	}
}