namespace Alex.Utils.Commands
{
	public class TextCommandProperty : CommandProperty
	{
		/// <inheritdoc />
		public TextCommandProperty(string name, bool required = true) : base(name, required, "text")
		{
			
		}
	}
}