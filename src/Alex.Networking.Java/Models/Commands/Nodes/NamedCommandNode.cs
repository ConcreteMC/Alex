namespace Alex.Networking.Java.Models.Commands.Nodes
{
	public class NamedCommandNode : CommandNode
	{
		public string Name { get; }
		/// <inheritdoc />
		public NamedCommandNode(CommandNodeType type, string name) : base(type)
		{
			Name = name;
		}
	}
}