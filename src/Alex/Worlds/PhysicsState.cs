using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Microsoft.Xna.Framework;

namespace Alex.Worlds
{
	public class PhysicsState
    {
	    public Vector3        Velocity { get; set; }
	    public PlayerLocation Position { get; set; }

	    public PhysicsState(Vector3 velocity, PlayerLocation position)
	    {
		    Velocity = velocity;
		    Position = position;
	    }
    }
}
