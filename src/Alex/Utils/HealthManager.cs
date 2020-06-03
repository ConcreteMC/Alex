using System;
using Alex.API.Utils;
using Alex.Entities;
using Microsoft.Xna.Framework;
using MathF = System.MathF;

namespace Alex.Utils
{
	public class HealthManager
	{
		private Entity Entity { get; }

		private float _health = 20;

		public float Health
		{
			get
			{
				return _health;
			}
			set
			{
				_health = MathF.Min(MaxHealth, value);
			}
		}

		public float MaxHealth { get; set; } = 20f;

		public int Hearts
		{
			get { return (int) Math.Ceiling(Health / 10d); }
		}

		public int MaxHearts
		{
			get { return (int) Math.Ceiling(MaxHealth / 10d); }
		}

		private int _hunger = 20;

		public int Hunger
		{
			get
			{
				return _hunger;
			}
			set
			{
				_hunger = Math.Min(MaxHunger, value);
			}
		}

		public int MaxHunger { get; set; } = 20;

		public float Saturation    { get; set; } = 20;
		public float MaxSaturation { get; set; } = 20;

		private float _exhaustion = 0f;

		public float Exhaustion
		{
			get
			{
				return _exhaustion;
			}
			set
			{
				_exhaustion = value;
			}
		}

		public float MaxExhaustion { get; set; } = 5f;

		public HealthManager(Entity entity)
		{
			Entity = entity;
		}

		private long _ticker = 0;

		public void Heal(int amount)
		{
			Health += amount * 10;
		}

		public void TakeHit(int amount)
		{
			Health -= amount * 10;
		}

		public void Exhaust(float amount)
		{
			Exhaustion += amount;
		}

		private DateTime       _lastMovementUpdate     = DateTime.UtcNow;
		private PlayerLocation _lastExhaustionPosition = new PlayerLocation();
		
		private void DoHealthAndExhaustion()
		{
			var elapsed = (DateTime.UtcNow - _lastMovementUpdate).TotalSeconds;

			var pos = Entity.KnownPosition;

			var distance = MathF.Abs(
				Vector3.DistanceSquared(
					new Vector3(pos.X, 0, pos.Z),
					new Vector3(_lastExhaustionPosition.X, 0, _lastExhaustionPosition.Z)));

			if (Entity.IsSprinting)
			{
				Exhaust(distance * 0.1f);
			}
			else if (Entity.IsInWater)
			{
				Exhaust(distance * 0.01f);
			}

			_ticker += 1;

			if (_ticker >= 80)
			{
				_ticker = 0;
			}

			if (_ticker == 0)
			{
				if (Hunger >= 18 && Health < MaxHealth)
				{
					Heal(1);
					Exhaust(3);
				}
				else if (Hunger <= 0 && _health > 1)
				{
					TakeHit(1);
				}
			}

			while (_exhaustion >= 4)
			{
				_exhaustion -= 4;

				if (Saturation > 0)
				{
					Saturation -= 1;
				}
				else
				{
					Hunger -= 1;
					// Saturation = 0;
					if (Hunger < 0) Hunger = 0;
				}
			}

			_lastExhaustionPosition = pos;
			_lastMovementUpdate = DateTime.UtcNow;
		}

		public void OnTick()
		{
			DoHealthAndExhaustion();
		}
	}
}