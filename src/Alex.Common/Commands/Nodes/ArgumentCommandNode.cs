using System.Collections.Generic;
using Alex.Common.Commands.Properties;

namespace Alex.Common.Commands.Nodes
{
	public class ArgumentCommandNode : NamedCommandNode
	{
		public string Parser { get; set; }
		public List<ArgumentParser> Parsers { get; set; }
		public string SuggestionType { get; set; } = null;
		/// <inheritdoc />
		public ArgumentCommandNode(string name) : base(CommandNodeType.Argument, name)
		{
			
		}
	}
}