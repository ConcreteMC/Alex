using Alex.Blocks.State;

namespace Alex.Blocks.Mapping
{
	public class PeBlockState : BlockState
	{
		public PeBlockState(BlockState original)
		{
			Original = original;
		}


		public BlockState Original { get; }
	}
	public static class BlockMapper
	{
		public static void Init(IProgressReceiver progressReceiver)
		{
			
		}
	}
}