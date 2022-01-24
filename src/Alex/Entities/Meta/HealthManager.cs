using System;
using Alex.Common.Utils.Vectors;
using Alex.Common.World;
using Alex.Entities.Components;
using Alex.Items;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Worlds;
using MathF = System.MathF;

namespace Alex.Entities.Meta
{
	public class HealthChangedEventArgs : EventArgs
	{
		public float Health { get; }
		public float MaxHealth { get; }

		public HealthChangedEventArgs(float health, float maxHealth)
		{
			Health = health;
			MaxHealth = maxHealth;
		}
	}

	public class ExhaustionChangedEventArgs : EventArgs
	{
		public float Exhaustion { get; }

		public ExhaustionChangedEventArgs(float health)
		{
			Exhaustion = health;
		}
	}

	public class SaturationChangedEventArgs : EventArgs
	{
		public float Saturation { get; }

		public SaturationChangedEventArgs(float health)
		{
			Saturation = health;
		}
	}

	public class HungerChangedEventArgs : EventArgs
	{
		public int Hunger { get; }
		public int MaxHunger { get; }

		public HungerChangedEventArgs(int hunger, int maxHunger)
		{
			Hunger = hunger;
			MaxHunger = maxHunger;
		}
	}

	public class AirChangedEventArgs : EventArgs
	{
		public short AirAvailable { get; }
		public short MaxAirAvailable { get; }

		public AirChangedEventArgs(short airAvailable, short maxAirAvailable)
		{
			AirAvailable = airAvailable;
			MaxAirAvailable = maxAirAvailable;
		}
	}

	public class HealthManager : EntityComponent, ITicked
	{
		//	private Entity Entity { get; }

		private float _health = 20f;
		private float _maxHealth = 20f;
		private int _maxHunger = 20;
		private short _availableAir = 300;
		private short _maxAir = 300;

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
			get { return (int)Math.Ceiling(Health / 10d); }
		}

		public int MaxHearts
		{
			get { return (int)Math.Ceiling(MaxHealth / 10d); }
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

		public float Saturation { get; set; } = 20;
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
				_exhaustion = Math.Clamp(value, 0f, MaxExhaustion);

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

		public int FireTick { get; set; }
		public int SuffocationTicks { get; set; }
		public int LavaTicks { get; set; }

		public HealthManager(Entity entity) : base(entity, "HealthManager")
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

		public void Regen(int amount)
		{
			Health += amount;
			if (Health > MaxHealth) Health = MaxHealth;
		}

		public bool IsDead => Health <= 0;

		public void Kill()
		{
			if (IsDead)
				return;

			Health = 0;
			//	IsDead = true;
		}

		public DamageCause LastDamageCause { get; protected set; } = DamageCause.Unknown;
		public float Absorption { get; set; }

		public void TakeHit(int damage, DamageCause cause = DamageCause.Unknown)
		{
			if (Entity is Player player && player.Gamemode != GameMode.Survival)
				return;


			if (CooldownTick > 0) return;

			//LastDamageSource = source;
			LastDamageCause = cause;

			if (Absorption > 0)
			{
				float abs = Absorption;
				abs -= damage;

				if (abs < 0)
				{
					Absorption = 0;
					damage = Math.Abs((int)Math.Floor(abs));
				}
				else
				{
					Absorption = abs;
					damage = 0;
				}
			}

			if (cause == DamageCause.Starving)
			{
				if (Entity.Level.Difficulty <= Difficulty.Easy && Hearts <= 10) return;
				if (Entity.Level.Difficulty <= Difficulty.Normal && Hearts <= 1) return;
			}

			Health -= damage;

			if (Health < 0)
			{
				//OnPlayerTakeHit(new HealthEventArgs(this, source, Entity));
				Health = 0;
				Kill();

				return;
			}


			IncreaseExhaustion(0.3f);

			//	if (source != null)
			//	{
			//		DoKnockback(source, tool);
			//	}

			CooldownTick = 10;

			//OnPlayerTakeHit(new HealthEventArgs(this, source, Entity));
		}

		protected virtual void DoKnockback(Entity source, Item tool) { }

		public void IncreaseExhaustion(float amount)
		{
			Exhaustion += amount;
			ProcessHunger();
		}

		private PlayerLocation _lastExhaustionPosition = new PlayerLocation();

		public void Move(float distance)
		{
			float movementStrainFactor = 0.01f; // Default for walking

			if (Entity.IsSneaking)
			{
				movementStrainFactor = 0.005f;
			}
			else if (Entity.IsSprinting)
			{
				movementStrainFactor = 0.1f;
			}

			Exhaustion += (distance * movementStrainFactor);

			ProcessHunger();
		}

		private void ProcessHunger()
		{
			if (Hunger > MaxHunger)
			{
				Hunger = MaxHunger;
			}

			if (Saturation > Hunger)
			{
				Saturation = Hunger;
			}

			while (Exhaustion >= 4)
			{
				Exhaustion -= 4;

				if (Saturation > 0)
				{
					Saturation -= 1;
				}
				else
				{
					Hunger -= 1;
					Saturation = 0;

					if (Hunger < 0) Hunger = 0; // Damage!
				}
			}
		}

		private void ProcessHungerTick()
		{
			if (Hunger <= 0)
			{
				_ticker++;

				if (_ticker % 800 == 0)
				{
					TakeHit(1, DamageCause.Starving);
				}
			}
			else if (Hunger > 18 && Health < MaxHealth)
			{
				_ticker++;

				if (Hunger >= 20 && Saturation > 0)
				{
					if (_ticker % 100 == 0)
					{
						if (Entity.Level.Difficulty != Difficulty.Hardcore)
						{
							IncreaseExhaustion(4);
							Regen(1);
						}
					}
				}
				else
				{
					if (_ticker % 800 == 0)
					{
						if (Entity.Level.Difficulty != Difficulty.Hardcore)
						{
							IncreaseExhaustion(4);
							Regen(1);
						}
					}
				}
			}
			else
			{
				_ticker = 0;
			}
		}

		private void DoHealthTick()
		{
			if (CooldownTick > 0) CooldownTick--;

			if (!Entity.IsSpawned) return;

			if (IsDead) return;

			if (Entity.Invulnerable) Health = MaxHealth;

			if (Health <= 0)
			{
				Kill();

				return;
			}

			if (Entity.HeadInWater)
			{
				AvailableAir--;

				if (AvailableAir <= 0)
				{
					if (Math.Abs(AvailableAir) % 10 == 0)
					{
						TakeHit(1, DamageCause.Drowning);
					}
				}

				if (Entity.IsOnFire)
				{
					Entity.IsOnFire = false;
					FireTick = 0;
				}
			}
			else
			{
				AvailableAir = MaxAir;

				if (Entity.HeadInWater)
				{
					Entity.HeadInWater = false;
				}
			}

			if (Entity.HeadInBlock)
			{
				if (SuffocationTicks <= 0)
				{
					TakeHit(1, DamageCause.Suffocation);
					SuffocationTicks = 10;
				}
				else
				{
					SuffocationTicks--;
				}
			}
			else
			{
				SuffocationTicks = 10;
			}

			if (Entity.IsInLava)
			{
				if (LastDamageCause.Equals(DamageCause.Lava))
				{
					FireTick += 2;
				}
				else
				{
					FireTick = 300;
					Entity.IsOnFire = true;
				}

				if (LavaTicks <= 0)
				{
					TakeHit(4, DamageCause.Lava);
					LavaTicks = 10;
				}
				else
				{
					LavaTicks--;
				}
			}
			else
			{
				LavaTicks = 0;
			}

			if (!Entity.IsInLava && Entity.IsOnFire)
			{
				FireTick--;

				if (FireTick <= 0)
				{
					Entity.IsOnFire = false;
				}

				if (Math.Abs(FireTick) % 20 == 0)
				{
					TakeHit(1, DamageCause.FireTick);
				}
			}
		}

		public int CooldownTick { get; set; }

		public void OnTick()
		{
			if (!Entity.IsSpawned)
				return;

			ProcessHungerTick();

			var pos = Entity.KnownPosition;

			var distance = MathF.Abs(
				Vector3.DistanceSquared(
					new Vector3(pos.X, 0, pos.Z),
					new Vector3(_lastExhaustionPosition.X, 0, _lastExhaustionPosition.Z)));

			if (MathF.Abs(distance) >= 0.005f)
			{
				Move(distance);
			}

			_lastExhaustionPosition = pos;

			DoHealthTick();
		}

		public void Reset()
		{
			Health = MaxHealth;
			AvailableAir = MaxAir;
			Entity.IsOnFire = false;
			FireTick = 0;
			SuffocationTicks = 10;
			LavaTicks = 0;
			//IsDead = false;
			CooldownTick = 0;
			LastDamageCause = DamageCause.Unknown;
			//	LastDamageSource = null;

			Hunger = MaxHunger;
			Saturation = MaxHunger;
			Exhaustion = 0;
		}
	}
}