using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.API.Blocks.State
{
    public interface IBlockStatePaletteResizer
    {
	    uint OnResize(int bits, IBlockState state);
	}
}
