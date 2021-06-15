using Microsoft.Xna.Framework;

namespace Alex.Entities.Components.Effects
{
	public class Effect
	{
		public const int MaxDuration = 0x7fffffff;

		public EffectType EffectId      { get; set; }
		public int        Duration      { get; set; } = -1;
		public int        Level         { get; set; }
		public bool       Particles     { get; set; }
		public Color      ParticleColor { get; set; } = Color.Black;

		protected Effect(EffectType id)
		{
			EffectId = id;
			Particles = true;
		}

		public virtual float Modify(float modifier)
		{
			return modifier;
		}
		
		public virtual void ApplyTo(Entity entity){}
		public virtual void Remove(Entity entity){}
		
		public virtual void OnTick(Entity entity)
		{
			if (Duration > 0 && Duration != MaxDuration) 
				Duration --;
		}

		public virtual bool HasExpired()
		{
			return Duration <= 0;
		}
		
		public override string ToString()
		{
			return $"EffectId: {EffectId}, Duration: {Duration}, Level: {Level}, Particles: {Particles}";
		}
	}
}