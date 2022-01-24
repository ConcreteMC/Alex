namespace Alex.Utils.Commands
{
	public class AskServerProperty : CommandProperty
	{
		/// <inheritdoc />
		public AskServerProperty(string name, bool required = true, string typeIdentifier = "Unknown") : base(
			name, required, typeIdentifier) { }
	}
}