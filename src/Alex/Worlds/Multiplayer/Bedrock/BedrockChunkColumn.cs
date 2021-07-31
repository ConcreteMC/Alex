using Alex.Worlds.Chunks;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BedrockChunkColumn : ChunkColumn
	{
		/// <inheritdoc />
		public BedrockChunkColumn(int x, int z, WorldSettings worldSettings) : base(x, z, worldSettings) { }

		/// <inheritdoc />
		public BedrockChunkColumn(int x, int z) : base(x, z) { }

		/// <inheritdoc />
		protected override ChunkSection CreateSection(bool storeSkylight, int storages)
		{
			return new BedrockChunkSection(storages);
		}
	}
}