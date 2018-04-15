using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.API
{
    public interface IPhysicsEntity
    {
	    double Height { get; set; }
	    double Width { get; set; }
	    double Length { get; set; }
	    double Drag { get; set; }
	    double Gravity { get; set; }
		float TerminalVelocity { get; set; }
		Vector3 Velocity { get; set; }

		void OnTick();
    }
}
