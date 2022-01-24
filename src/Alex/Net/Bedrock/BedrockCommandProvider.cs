using Alex.Utils.Commands;
using Alex.Worlds;

namespace Alex.Net.Bedrock
{
	public class BedrockCommandProvider : CommandProvider
	{
		/// <inheritdoc />
		public override void DoMatch(string input, OnCommandMatch callback) { }

		/// <inheritdoc />
		public BedrockCommandProvider(World world) : base(world) { }
	}
}