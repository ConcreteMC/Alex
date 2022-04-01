using System.Collections.Generic;
using Alex.Networking.Java.Commands.Nodes;

namespace Alex.Networking.Java.Commands
{
	public class CommandTreeBuilder
	{
		private List<CommandNode> _nodes;
		private int _rootIndex = 0;

		public int RootIndex => _rootIndex;

		public CommandTreeBuilder()
		{
			_nodes = new List<CommandNode>();
			_rootIndex = Add(new CommandNode(CommandNodeType.Root));
		}

		public int Add(CommandNode node)
		{
			int index = _nodes.Count;
			_nodes.Add(node);

			return index;
		}

		public CommandNode[] ExportNodes()
		{
			return _nodes.ToArray();
		}
	}
}