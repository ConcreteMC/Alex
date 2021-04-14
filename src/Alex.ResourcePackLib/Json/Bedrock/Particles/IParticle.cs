using Microsoft.Xna.Framework;

namespace Alex.ResourcePackLib.Json.Bedrock.Particles
{
	public interface IParticle
	{
		/// <summary>
		///		The velocity of the particle
		/// </summary>
		Vector3 Velocity { get; set; }
		
		/// <summary>
		///		The position of the particle
		/// </summary>
		Vector3 Position { get; set; }
		
		/// <summary>
		///		The acceleration applied to the particle
		/// </summary>
		Vector3 Acceleration { get; set; }
		
		/// <summary>
		///		The drag co-efficient applied to the particle
		/// </summary>
		float DragCoEfficient { get; set; }
		
		/// <summary>
		///		How long this particle has been visible for
		/// </summary>
		double Lifetime { get; set; }
		
		/// <summary>
		///		How long this particle can be visible for
		/// </summary>
		double MaxLifetime { get; set; }
		
		/// <summary>
		///		The total amount of frames for this particle.
		/// </summary>
		float FrameCount { get; set; }
		
		/// <summary>
		///		The position of the sprite on the spritesheet
		/// </summary>
		Vector2 UvPosition { get; set; }
		
		/// <summary>
		///		The size of the sprite on the spritesheet
		/// </summary>
		Vector2 UvSize { get; set; }
		
		/// <summary>
		///		Specifies the x and y size of the billboard.
		/// </summary>
		Vector2 Size { get; set; }
	}
}