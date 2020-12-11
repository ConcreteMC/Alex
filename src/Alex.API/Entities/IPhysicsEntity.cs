using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Entities
{
    public interface IPhysicsEntity
    {
	    double Height { get; set; }
	    double Width { get; set; }
	    double Length { get; }
	    double Drag { get; set; }
	    double Gravity { get; set; }
		float TerminalVelocity { get; set; }
		Vector3 Velocity { get; set; }

		//PhysicsState State         { get; set; }
		PhysicsState PreviousState { get; set; }
		
		void OnTick();
    }

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
