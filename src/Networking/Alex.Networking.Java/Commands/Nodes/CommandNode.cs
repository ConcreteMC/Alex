namespace Alex.Networking.Java.Commands.Nodes
{
	public class CommandNode
	{
		public CommandNodeType NodeType { get; }
		public bool IsExecutable { get; set; } = false;
		public bool HasRedirect { get; set; } = false;
		public int RedirectIndex { get; set; } = -1;

		public CommandNode(CommandNodeType type)
		{
			NodeType = type;
		}

		public int[] Children { get; set; }
	}
}