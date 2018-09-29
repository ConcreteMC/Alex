using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Blocks
{
    public class Leaves : Block
    {
	    public Leaves(uint blockStateId) : base(blockStateId)
	    {
		    Solid = true;
		    Transparent = true;
		    IsReplacible = false;
		    IsFullBlock = true;
		    IsFullCube = true;
		}
    }
}
