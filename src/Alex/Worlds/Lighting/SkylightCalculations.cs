using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.World;


namespace Alex.Worlds.Lighting
{
    public class SkylightCalculations
    {
		private IWorld World { get; }
	    public SkylightCalculations(IWorld world)
	    {
		    World = world;
	    }


		public void CalculateSkylight(ChunkColumn column)
		{
		
		}
	}
}
