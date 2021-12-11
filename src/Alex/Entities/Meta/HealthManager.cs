using System;
using Alex.Common.Utils.Vectors;
using Alex.Common.World;
using Alex.Entities.Components;
using Microsoft.Xna.Framework;
using MathF = System.MathF;

namespace Alex.Entities.Meta
{
	public class HealthChangedEventArgs : EventArgs
	{
		public float Health { get; }
		public float MaxHealth { get; }
		
		public HealthChangedEventArgs(float health, float maxHealth) {
			Health = health;
			MaxHealth = maxHealth;
		}
	}
	
	public class ExhaustionChangedEventArgs : EventArgs
	{
		public float Exhaustion { get; }

		public ExhaustionChangedEventArgs(float health) {
			Exhaustion = health;
		}
	}
	
	public class SaturationChangedEventArgs : EventArgs
	{
		public float Saturation { get; }

		public SaturationChangedEventArgs(float health) {
			Saturation = health;
		}
	}
	
	public class HungerChangedEventArgs : EventArgs
	{
		public int Hunger { get; }
		public int MaxHunger { get; }
		
		public HungerChangedEventArgs(int hunger, int maxHunger) {
			Hunger = hunger;
			MaxHunger = maxHunger;
		}
	}
	
	public class AirChangedEventArgs : EventArgs
	{
		public short AirAvailable { get; }
		public short MaxAirAvailable { get; }
		
		public AirChangedEventArgs(short airAvailable, short maxAirAvailable) {
			AirAvailable = airAvailable;
			MaxAirAvailable = maxAirAvailable;
		}
	}
	
	public class HealthManager : EntityComponent, ITicked
	{
	//	private Entity Entity { get; }

		private float _health = 20;

		private void InvokeHealthUpdate()
		{
			OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(Health, MaxHealth));
		}
		
		public float Health
		{
			get
			{
				return _health;
			}
			set
			{
				_health = MathF.Min(MaxHealth, value);
				InvokeHealthUpdate();
			}
		}

		public float MaxHealth
		{
			get => _maxHealth;
			set
			{
				_maxHealth = value;
				InvokeHealthUpdate();
			}
		}

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
				InvokeHunger();
			}
		}

		public int MaxHunger
		{
			get => _maxHunger;
			set
			{
				_maxHunger = value;
				InvokeHunger();
			}
		}

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
				var previous = _exhaustion;
				_exhaustion = value;
				
				OnExhaustionChanged?.Invoke(this, new ExhaustionChangedEventArgs(value));
			}
		}

		public float MaxExhaustion { get; set; } = 5f;

		public short AvailableAir
		{
			get => _availableAir;
			set
			{
				_availableAir = value;
				InvokeAirUpdate();
			}
		}

		public short MaxAir
		{
			get => _maxAir;
			set
			{
				_maxAir = value;
				InvokeAirUpdate();
			}
		}

		public bool IsDying => Health * (10d / MaxHealth) < 1d;
		
		/// <summary>
		///		Returns the ticks since the entity started dying
		/// </summary>
		public int DyingTime { get; private set; } = 0;

		public EventHandler<HealthChangedEventArgs> OnHealthChanged;
		public EventHandler<HungerChangedEventArgs> OnHungerChanged;
		public EventHandler<ExhaustionChangedEventArgs> OnExhaustionChanged;
		public EventHandler<SaturationChangedEventArgs> OnSaturationChanged;
		public EventHandler<AirChangedEventArgs> OnAvailableAirChanged;
		
		public HealthManager(Entity entity) : base(entity)
		{
			//Entity = entity;
		}

		private void InvokeAirUpdate()
		{
			OnAvailableAirChanged?.Invoke(this, new AirChangedEventArgs(_availableAir, _maxAir));
		}

		private void InvokeHunger()
		{
			OnHungerChanged?.Invoke(this, new HungerChangedEventArgs(Hunger, MaxHunger));
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
		private float _maxHealth = 20f;
		private int _maxHunger = 20;
		private short _availableAir = 300;
		private short _maxAir = 300;

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
					//Heal(1);
					Exhaust(3);
				}
				else if (Hunger <= 0 && _health > 1)
				{
					//TakeHit(1);
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

		public void Reset()
		{
			AvailableAir = MaxAir;
			Health = MaxHealth;
			Exhaustion = 0f;
			Saturation = MaxSaturation;
			Hunger = MaxHunger;
		}
	}
}