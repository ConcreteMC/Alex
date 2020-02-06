using MiNET.Utils;

namespace Alex.Utils
{
    public static class MiNETExtensions
    {
        public static string Value(this IBlockState state)
        {
            if (state is BlockStateInt bsi)
            {
                return bsi.Value.ToString();
            }
            
            if (state is BlockStateByte bsb)
            {
                return bsb.Value.ToString();
            }
            
            if (state is BlockStateString bss)
            {
                return bss.Value;
            }

            return null;
        }
    }
}