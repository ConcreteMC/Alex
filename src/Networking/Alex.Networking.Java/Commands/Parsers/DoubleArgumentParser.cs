namespace Alex.Networking.Java.Commands.Parsers
{
	public class DoubleArgumentParser : RangeArgumentParser<double>
	{
		/// <inheritdoc />
		public DoubleArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		protected override bool TryParse(string input, out double answer)
		{
			return double.TryParse(input, out answer);
		}
	}
}