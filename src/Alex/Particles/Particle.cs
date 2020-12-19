using Microsoft.Xna.Framework;

namespace Alex.Particles
{
	public class Particle
	{
		public int Age      { get; private set; } = 0;
		public int Lifetime { get; private set; } = 1200; //1 minute
		
		public Vector3 Velocity { get; set; }
		public Vector3 Position { get; set; }

		public string Texture { get; }

		public Particle(string texture)
		{
			Texture = texture;
			Velocity = Vector3.Zero;
			Position = Vector3.Zero;
		}

		public void Tick()
		{
			Age++;

			OnTick();
		}

		protected virtual void OnTick() { }
	}
}