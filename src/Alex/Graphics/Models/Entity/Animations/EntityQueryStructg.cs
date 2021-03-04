using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Items;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class EntityQueryStruct : QueryStruct
	{
		private Entities.Entity Entity { get; }
		public EntityQueryStruct(Entities.Entity entity) : base()
		{
			Entity = entity;
			
			BuildStruct();
		}

		private void BuildStruct()
		{
			Functions.Add("life_time", mo => Entity.LifeTime.TotalSeconds);
			Functions.Add("position_delta", mo =>
			{
				var delta = Entity.KnownPosition;
				double amount = 0d;
				switch (mo.GetInt(0))
				{
					case 0: //X-Axis
						
						break;
					case 1: //Y-Axis
						
						break;
					case 2: //Z-Axis
						
						break;
				}
				
				return new DoubleValue(amount);
			});
			Functions.Add("position", GetPosition);
			
			Functions.Add("main_hand_item_use_duration", mo => DoubleValue.Zero);
			Functions.Add("main_hand_item_max_duration", mo => DoubleValue.Zero);
			
			Functions.Add("modified_distance_moved", mo => Entity.Movement.DistanceMoved);
			
			Functions.Add("modified_move_speed", mo => (1f / (Entity.CalculateMovementSpeed() * 43f)) * (Entity.Movement.MetersPerSecond));
			
			Functions.Add("delta_time", mo => _deltaTime.TotalSeconds);
			Functions.Add("ground_speed", mo => Entity.Movement.MetersPerSecond);

			Functions.Add("vertical_speed", mo => Entity.Movement.VerticalSpeed);
			Functions.Add("time_of_day", mo => ((1f / 24000f) * Entity.Level.TimeOfDay));

			Functions.Add("is_alive", mo => Entity.HealthManager.Health > 0);
			Functions.Add("is_on_ground", mo => Entity.KnownPosition.OnGround);
			Functions.Add("is_riding", mo => Entity.IsRiding);
			Functions.Add("is_sneaking", mo => Entity.IsSneaking);
			Functions.Add("is_charging", mo => Entity.IsAngry);
			Functions.Add("is_tamed", mo => Entity.IsTamed);
			Functions.Add("is_using_item", mo => Entity.IsUsingItem);
			Functions.Add("is_wall_climbing", mo => Entity.IsWallClimbing);
			Functions.Add("is_onfire", mo => Entity.IsOnFire);
			Functions.Add("is_on_fire", mo => Entity.IsOnFire);
			Functions.Add("is_sprinting", mo => Entity.IsSprinting);
			Functions.Add("is_on_screen", mo => Entity.IsRendered);
			Functions.Add("is_moving", mo => Entity.IsMoving);
			Functions.Add("is_leashed", mo => Entity.IsLeashed);
			Functions.Add("is_interested", mo => Entity.IsInterested);
			Functions.Add("is_in_love", mo => Entity.IsInLove);
			Functions.Add("is_ignited", mo => Entity.IsIgnited);
			Functions.Add("is_eating", mo => Entity.IsEating);
			Functions.Add("is_chested", mo => Entity.IsChested);
			Functions.Add("is_breathing", mo => Entity.IsBreathing);
			Functions.Add("is_baby", mo => Entity.IsBaby);
			Functions.Add("is_stackable", mo => Entity.IsStackable);
			Functions.Add("is_in_water", mo => Entity.IsInWater);
			Functions.Add("is_in_water_or_rain", mo => (Entity.IsInWater || Entity.Level.Raining));
			Functions.Add("is_sheared", mo => Entity.IsSheared);
			Functions.Add("is_silent", mo => Entity.IsSilent);
			Functions.Add("is_powered", mo => Entity.IsPowered);
			Functions.Add("is_elder", mo => Entity.IsElder);
			Functions.Add("is_resting", mo => Entity.IsResting);
			Functions.Add("is_tempted", mo => Entity.IsTempted);
			Functions.Add("is_critical", mo => Entity.IsCritical);
			Functions.Add("is_converting", mo => Entity.IsConverting);
			Functions.Add("is_standing", mo => (Entity.IsStanding));
			Functions.Add("is_sleeping", mo => Entity.IsSleeping);
			Functions.Add("is_swimming", mo => Entity.IsSwimming);
			Functions.Add("is_sitting", mo => Entity.IsSitting);
			Functions.Add("is_blocking", mo => Entity.IsBlocking);
			Functions.Add("blocking", mo => Entity.IsBlocking);
			Functions.Add("has_gravity", mo => Entity.IsAffectedByGravity);
			Functions.Add("has_collision", mo => Entity.HasCollision);
			Functions.Add("can_fly", mo => Entity.CanFly);
			
			Functions.Add("has_target", mo => Entity.TargetEntityId != -1);
			Functions.Add("has_owner", mo => Entity.OwnerEntityId != -1);
			Functions.Add("target_x_rotation", GetTargetXRotation);
			Functions.Add("target_y_rotation", GetTargetYRotation);
			Functions.Add("is_selected_item", mo => (Entity.Inventory.MainHand.Count > 0));
			Functions.Add("is_item_equipped", IsItemEquipped);
			Functions.Add("get_equipped_item_name", GetEquippedItemName);
			Functions.Add("actor_count", mo => Entity.Level.EntityManager.EntitiesRendered);
				//Functions.Add("body_x_rotation", );
		}

		private object IsItemEquipped(MoParams mo)
		{
			bool isMainHand = true;

			if (mo.Contains(0))
			{
				var val = mo.Get(0);

				if (val is StringValue sv)
				{
					if (sv.Value == "off_hand")
						isMainHand = false;
				}
				else if (val is DoubleValue dv)
				{
					if (dv.Value > 0)
						isMainHand = false;
				}
			}
			Item item = isMainHand ? Entity.Inventory.MainHand : Entity.Inventory.OffHand;

			return item.Count > 0;
		}

		private object GetTargetYRotation(MoParams mo)
		{
			var targetId = Entity.TargetEntityId;
			if (targetId == -1)
				return 0d;

			return Entity.TargetRotation.Y;
		}
		
		private object GetTargetXRotation(MoParams mo)
		{
			var targetId = Entity.TargetEntityId;
			if (targetId == -1)
				return 0d;

			return Entity.TargetRotation.X;
		}

		private object GetPosition(MoParams mo)
		{
			double amount = 0d;

			switch (mo.GetInt(0))
			{
				case 0: //X-Axis
					amount = Entity.KnownPosition.X;

					break;

				case 1: //Y-Axis
					amount = Entity.KnownPosition.Y;

					break;

				case 2: //Z-Axis
					amount = Entity.KnownPosition.Z;

					break;
			}

			return new DoubleValue(amount);
		}

		private object GetEquippedItemName(MoParams mo)
		{
			bool isOffHand = false;

			if (mo.Contains(0))
			{
				var firstArgument = mo.Get(0);

				if (firstArgument is StringValue sv)
				{
					if (!sv.Value.Equals("main_hand")) isOffHand = true;
				}
				else if (firstArgument is DoubleValue dv)
				{
					if (dv.Value > 0) isOffHand = true;
				}
			}

			if (mo.Contains(1)) { }

			Item item = null;

			if (!isOffHand) item = Entity.Inventory.MainHand;
			else item = Entity.Inventory.OffHand;

			if (item?.Name == null) return new StringValue("air");

			return new StringValue(item.Name.Replace("minecraft:", ""));
		}

		private TimeSpan _deltaTime = TimeSpan.Zero;
		public void Tick(TimeSpan deltaTime)
		{
			_deltaTime = deltaTime;
		}
	}
}