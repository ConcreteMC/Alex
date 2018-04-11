using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Items;

namespace Alex.Items
{
    public class Item : IItem
	{
	    public int MaxStackSize { get; set; }= 64;
    }
}
