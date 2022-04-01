namespace Alex.Networking.Java.Commands.Parsers
{
	public class FloatArgumentParser : RangeArgumentParser<float>
	{
		/// <inheritdoc />
		public FloatArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		protected override bool TryParse(string input, out float answer)
		{
			return float.TryParse(input, out answer);
		}
	}
}