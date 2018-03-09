using Alex.API.Blocks.State;

namespace Alex.API.World
{
	public interface IBlockStatePalette
	{
		int IdFor(IBlockState state);
		IBlockState GetBlockState(int indexKey);

		/*@SideOnly(Side.CLIENT)
		void read(PacketBuffer buf);

		void write(PacketBuffer buf);*/

		int GetSerializedSize();
	}
}
