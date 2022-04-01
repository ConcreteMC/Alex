namespace Alex.Networking.Java.Commands.Nodes
{
	public class LiteralCommandNode : NamedCommandNode
	{
		/// <inheritdoc />
		public LiteralCommandNode(string name) : base(CommandNodeType.Literal, name) { }
	}
}