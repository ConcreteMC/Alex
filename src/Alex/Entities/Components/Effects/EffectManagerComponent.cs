using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Components.Effects
{
	public class EffectManagerComponent : ITickedEntityComponent
	{
		private readonly Entity _entity;
		private readonly ConcurrentDictionary<EffectType, Effect> _effects = new ConcurrentDictionary<EffectType, Effect>();
		public EffectManagerComponent(Entity entity)
		{
			_entity = entity;
		}
		
		/// <inheritdoc />
		public void Update(GameTime gameTime)
		{
			
		}

		/// <inheritdoc />
		public void OnTick()
		{
			foreach (var effect in _effects.Values.ToArray())
			{
				effect.OnTick(_entity);

				if (effect.HasExpired())
					RemoveEffect(effect.EffectId);
			}
		}

		public float ApplyEffect(EffectType type, float modifier)
		{
			if (TryGetEffect(type, out var effect))
			{
				return effect.Modify(modifier);
			}
			return modifier;
		}
		
		public void AddOrUpdateEffect(Effect effect)
		{
			var effect1 = effect;
			
			effect = _effects.AddOrUpdate(effect.EffectId, effect, (type, e) => effect1);
			effect?.ApplyTo(_entity);
		}

		public void RemoveEffect(EffectType effectType)
		{
			if (_effects.TryRemove(effectType, out var removed))
			{
				removed.Remove(_entity);
			}
		}

		public bool TryGetEffect(EffectType type, out Effect effect)
		{
			return _effects.TryGetValue(type, out effect);
		}
		
		public bool TryGetEffect<T>(EffectType type, out T effect) where T : Effect
		{
			if (_effects.TryGetValue(type, out var temp))
			{
				if (temp is T t)
				{
					effect = t;
					return true;
				}
			}

			effect = null;

			return false;
		}

		public IEnumerable<Effect> AppliedEffects()
		{
			foreach (var effect in _effects.Values.ToArray())
			{
				yield return effect;
			}
		}
	}
}