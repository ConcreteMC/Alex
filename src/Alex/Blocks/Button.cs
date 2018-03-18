using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Blocks
{
    public class Button : Block
    {
	    public Button(uint id) : base(id)
	    {
		    Transparent = true;
		    Solid = true;
		    IsFullCube = false;

			/*
			 * Solid = false;
			Transparent = true;
			IsReplacible = false;
			 */
		}
	}
}
