using Alex.API.Blocks.State;

namespace Alex.API.World
{
	public interface IBlockStatePalette
	{
		uint IdFor(IBlockState state);
		IBlockState GetBlockState(uint indexKey);

		/*@SideOnly(Side.CLIENT)
		void read(PacketBuffer buf);

		void write(PacketBuffer buf);*/

		int GetSerializedSize();
	}
}
