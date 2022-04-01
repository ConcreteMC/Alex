namespace Alex.Networking.Java.Commands.Parsers
{
	public class IntegerArgumentParser : RangeArgumentParser<int>
	{
		/// <inheritdoc />
		public IntegerArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		protected override bool TryParse(string input, out int answer)
		{
			return int.TryParse(input, out answer);
		}
	}
}