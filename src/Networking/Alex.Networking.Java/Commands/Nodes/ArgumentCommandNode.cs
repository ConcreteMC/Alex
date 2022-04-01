using Alex.Networking.Java.Commands.Parsers;

namespace Alex.Networking.Java.Commands.Nodes
{
	public class ArgumentCommandNode : NamedCommandNode
	{
		public ArgumentParser Parser { get; set; }

		//public List<ArgumentParser> Parsers { get; set; }
		public string SuggestionType { get; set; } = null;

		/// <inheritdoc />
		public ArgumentCommandNode(string name) : base(CommandNodeType.Argument, name) { }
	}
}