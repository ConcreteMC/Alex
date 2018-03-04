using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.ResourcePackLib.Json.Models
{
    public class EntityModelBone
    {
		public string Name { get; set; }
		public JVector3 Pivot { get; set; }
	    public bool NeverRender { get; set; } = false;
		public JVector3 Rotation { get; set; }
		public EntityModelCube[] Cubes { get; set; }
    }
}
