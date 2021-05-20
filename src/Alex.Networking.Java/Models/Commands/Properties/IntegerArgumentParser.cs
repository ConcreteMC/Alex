namespace Alex.Networking.Java.Models.Commands.Properties
{
	public class IntegerArgumentParser : RangeArgumentParser<int>
	{
		/// <inheritdoc />
		public IntegerArgumentParser(string name) : base(name) { }
	}
}