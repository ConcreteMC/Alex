using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.ResourcePackLib.Json.Models
{
    public class EntityModel
    {
		public string Name { get; set; }
	    public int TextureWidth = 32;
	    public int TextureHeight = 16;
		public EntityModelBone[] Bones { get; set; }
	}
}
