namespace Alex.Entities.Effects
{
	public class NightVisionEffect : Effect
	{
		/// <inheritdoc />
		public NightVisionEffect() : base(EffectType.NightVision)
		{
			
		}

		/// <inheritdoc />
		public override void ApplyTo(Entity entity)
		{
			//TODO: Apply to rendering, caves should light up etc see 
			base.ApplyTo(entity);
		}
	}
}