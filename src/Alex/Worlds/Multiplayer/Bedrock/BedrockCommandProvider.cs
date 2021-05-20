using Alex.Utils;
using Alex.Utils.Commands;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BedrockCommandProvider : CommandProvider
	{
		/// <inheritdoc />
		public override void DoMatch(string input, OnCommandMatch callback)
		{
			
		}

		/// <inheritdoc />
		public BedrockCommandProvider(World world) : base(world) { }
	}
}