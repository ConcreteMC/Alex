using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Items;

namespace Alex.Items
{
    public class Item : IItem
	{
		public string DisplayName { get; set; }
	    public int MaxStackSize { get; set; }= 64;
    }
}
