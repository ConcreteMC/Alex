using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.API
{
    public interface IPhysicsEntity
    {
	    double Height { get; set; }
	    double Width { get; set; }
	    double Length { get; set; }
	    double Drag { get; set; }
	    double Gravity { get; set; }

		void OnTick();
    }
}
