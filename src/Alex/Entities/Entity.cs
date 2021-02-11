using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.Api;
using Alex.API;
using Alex.API.Data.Servers;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Network;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Entities.Effects;
using Alex.Entities.Meta;
using Alex.Entities.Properties;
using Alex.Gamestates;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.Graphics.Models.Items;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Alex.Utils.Inventories;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Java;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using Effect = Alex.Entities.Effects.Effect;
using MathF = System.MathF;
using MetadataByte = Alex.Networking.Java.Packets.Play.MetadataByte;
using MetadataFloat = MiNET.Utils.MetadataFloat;
using MetadataString = MiNET.Utils.MetadataString;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Entities
{
	public class Entity
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Entity));

		private EntityModelRenderer _modelRenderer;

		public EntityModelRenderer ModelRenderer
		{
			get
			{
				return _modelRenderer;
			}
			set
			{
				ItemRenderer = null;
				_modelRenderer = value;
				
				UpdateModelParts();
				OnModelUpdated();
				CheckHeldItem();
			}
		}
		
		public EntityMovement Movement { get; }

		public World Level { get; set; }
		
		public long EntityId  { get; internal set; }
		public bool IsSpawned { get; set; } = false;

		private        PlayerLocation _knownPosition = new PlayerLocation();

		public virtual PlayerLocation KnownPosition
		{
			get
			{
				return _knownPosition;
			}
			set
			{
				_knownPosition = value;
				_boundingBox = GetBoundingBox(value);
			}
		}

		private PlayerLocation _renderLocation;
		internal virtual PlayerLocation RenderLocation
		{
			get
			{
				return HasPhysics ? _renderLocation : KnownPosition;
			}
			set
			{
				if (HasPhysics)
				{
					_renderLocation = value;
				}
				else
				{
					KnownPosition = value;
				}
			}
		}

		public float   Slipperines { get; set; } = 0.6f;
		public Vector3 Velocity    { get; set; } = Vector3.Zero;
		
		//public HealthManager HealthManager { get; set; }
		public string NameTag { get; set; }

		public virtual bool NoAi { get; set; } = false;
		public bool HideNameTag { get; set; } = false;
		public bool Silent { get; set; }

		public bool HeadInBlock  { get; set; } = false;
		public bool AboveWater   { get; set; } = false;
		public bool HeadInWater  { get; set; } = false;
		public bool FeetInWater  { get; set; } = false;
		public bool IsInWater    { get; set; } = false;
		
		public bool HeadInLava  { get; set; } = false;
		public bool FeetInLava  { get; set; } = false;
		public bool IsInLava     { get; set; } = false;
		public bool IsOutOfWater => !IsInWater;
		public bool Invulnerable { get; set; } = false;

		public long Age { get; set; }

		private float _scale = 1f;

		public float Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;

				ScaleChanged();
			}
		}
		public double Height { get; set; } = 1;
		public double Width  { get; set; } = 1;
		
		public double Drag   { get; set; } = 0.02f;// 8f;

		public double Gravity { get; set; } = 0.08f; //16.8f; //9.81f; //1.6f;
		//Drag & gravity etc is Vanilla * 400
		
		public float TerminalVelocity { get; set; } = 78.4f;
		
		public MiNET.Utils.UUID UUID { get; set; }

		public bool CanFly { get; set; } = false;
		public bool IsFlying { get; set; } = false;

		public bool IsCollidingWithWorld { get; set; } = false;
		public bool AlwaysTick { get; set; } = false;

		private bool _isRendered = false;

		public bool IsRendered
		{
			get
			{
				return _isRendered;
			}
			internal set
			{
				var oldValue = _isRendered;
				_isRendered = value;

				if (value && !oldValue && !AlwaysTick)
				{
					Velocity = Vector3.Zero;
				}
			}
		}

		public NetworkProvider Network { get; set; }
		public Inventory Inventory { get; protected set; }

		public IItemRenderer ItemRenderer
		{
			get => _itemRenderer;
			private set
			{
				var oldValue = _itemRenderer;
				var newValue = value;
				//_itemRenderer = value;

				if (oldValue != null)
				{
					oldValue.Parent?.Remove(oldValue);
				}

				if (newValue != null)
				{
					EntityModelRenderer.ModelBone arm = null;

					if (_rightItemModel != null)
					{
						arm = _rightItemModel;
					}
					else if (_rightArmModel != null)
					{
						arm = _rightArmModel;
					}

					arm?.AddChild(value);
				}

				_itemRenderer = newValue;
				
				UpdateItemPosition();
			}
		}

		protected EntityModelRenderer.ModelBone _leftArmModel;
		protected EntityModelRenderer.ModelBone _leftItemModel;
		protected EntityModelRenderer.ModelBone _rightArmModel;
		protected EntityModelRenderer.ModelBone _rightItemModel;
		
		protected EntityModelRenderer.ModelBone _leftLegModel;
		protected EntityModelRenderer.ModelBone _rightLegModel;

		protected EntityModelRenderer.ModelBone _body;
		protected EntityModelRenderer.ModelBone _head;
		
		public  HealthManager HealthManager { get; }
		
		private bool          _isFirstPersonMode = false;

		public bool IsFirstPersonMode
		{
			get
			{
				return _isFirstPersonMode;
			}
			set
			{
				if (value != _isFirstPersonMode)
				{
					_isFirstPersonMode = value;
					UpdateItemPosition();
					//CheckHeldItem();
				}
			}
		}

		private Dictionary<string, EntityProperty> _entityProperties = new Dictionary<string, EntityProperty>()
		{
			
		};

		public IReadOnlyDictionary<string, EntityProperty> EntityProperties
		{
			get => _entityProperties;
			//set => _entityProperties = value;
		}
		
		public Entity(World level, NetworkProvider network)
		{
			Network = network;

            EntityId = -1;
            Level = level;
            KnownPosition = new PlayerLocation();
            RenderLocation = new PlayerLocation();
            //Inventory = new Inventory(46);
            SetInventory(new Inventory(46));
            //	HealthManager = new HealthManager(this);

            HideNameTag = false;
			ServerEntity = true;
			IsAffectedByGravity = true;
			
			HealthManager = new HealthManager(this);
			UUID = new MiNET.Utils.UUID(Guid.NewGuid().ToByteArray());
			
			//BaseMovementSpeed = 4.317D;
			
			AddOrUpdateProperty(new FlyingSpeedProperty(this));
			AddOrUpdateProperty(new MovementSpeedProperty(this));
			
			Movement = new EntityMovement(this);
		}

		public double FlyingSpeed
		{
			get
			{
				return _entityProperties[Networking.Java.Packets.Play.EntityProperties.FlyingSpeed].Value;
			}
			set
			{
				_entityProperties[Networking.Java.Packets.Play.EntityProperties.FlyingSpeed].Value = value;
			}
		}
		
		//private double
		public double MovementSpeed
		{
			get
			{
				return _entityProperties[Networking.Java.Packets.Play.EntityProperties.MovementSpeed].Value;
			}
			set
			{
				_entityProperties[Networking.Java.Packets.Play.EntityProperties.MovementSpeed].Value = value;
			}
		}

		public void AddOrUpdateProperty(EntityProperty property)
		{
			//Log.Info($"Update prop: {property.Key}= {property.Value}");
			if (_entityProperties.TryAdd(property.Key, property))
			{
				/*_entityProperties[property.Key].Value = property.Value;
				
				foreach (var modifier in property.Modifiers)
				{
					_entityProperties[property.Key].ApplyModifier(modifier.Value);
				}*/
			//	_entityProperties[property.Key].ApplyModifier();
			}
			else
			{
				_entityProperties[property.Key] = property;
			}
		}

		public double CalculateMovementSpeed()
		{
			var modifier =
				(_entityProperties[
					IsFlying ? Networking.Java.Packets.Play.EntityProperties.FlyingSpeed :
						Networking.Java.Packets.Play.EntityProperties.MovementSpeed]).Calculate();

			if (IsSneaking)
				modifier *= 0.29997683577f;
			
			return (modifier);
		}

		private bool _skipRendering = false;
		private void ScaleChanged()
		{
			if (ModelRenderer != null)
			{
				ModelRenderer.Scale = _scale;

				if (_scale <= 0.1f)
				{
					_skipRendering = true;
				}
				else
				{
					_skipRendering = false;
				}
			}
		}

		public EventHandler<Inventory> OnInventoryChanged;
		public void SetInventory(Inventory inventory)
		{
			var oldInventory = Inventory;

			if (oldInventory != null)
			{
				oldInventory.SlotChanged -= OnInventorySlotChanged;
				oldInventory.SelectedHotbarSlotChanged -= InventoryOnSelectedHotbarSlotChanged;
			}

			Inventory = inventory;
			//	HealthManager = new HealthManager(this);
			
			Inventory.SlotChanged += OnInventorySlotChanged;
			Inventory.SelectedHotbarSlotChanged += InventoryOnSelectedHotbarSlotChanged;

			OnInventoryChanged?.Invoke(this, inventory);
			
			CheckHeldItem();
		}

        private void CheckHeldItem()
        {
            var inHand = Inventory.MainHand;

            if ((inHand == null || inHand.Count == 0 || inHand.Id <= 0) && ItemRenderer != null && ModelRenderer != null)
            {
	            ItemRenderer = null;
                return;
            }

            if (inHand != null)
            {
	            //if (!string.IsNullOrWhiteSpace(inHand.Name))
	            {
		            var renderer = inHand?.Renderer;
		            if (renderer == null)
		            { 
			            //Log.Warn($"No renderer for item: {inHand.Name}");
			            return;
		            }

		            if (renderer != ItemRenderer)
		            {
			            renderer = renderer.Clone();
			            
			            ItemRenderer = renderer;
		            }
	            }
            }
            else
            {
                if (ItemRenderer != null)
                {
	               // ItemRenderer?.Parent?.Remove(ItemRenderer);
	                
	                ItemRenderer = null;
                }
            }
        }

        protected virtual void UpdateItemPosition()
        {
	        var renderer = ItemRenderer;

	        if (renderer == null)
		        return;
	        
	        renderer.DisplayPosition = DisplayPosition.ThirdPersonRightHand;
        }

        private void InventoryOnSelectedHotbarSlotChanged(object? sender, SelectedSlotChangedEventArgs e)
        {
            CheckHeldItem();
        }

		protected virtual void OnInventorySlotChanged(object sender, SlotChangedEventArgs e)
		{
			CheckHeldItem();
		}

		public bool IsOnFire { get; set; } = false;
		public bool IsSneaking { get; set; }
		public bool IsRiding { get; set; }
		public bool IsSprinting { get; set; }
		public bool IsUsingItem { get; set; }
		public bool IsInvisible { get; set; }
		public bool IsTempted { get; set; }
		public bool IsInLove { get; set; }
		
		public bool IsPowered { get; set; }
		public bool IsIgnited { get; set; }
		public bool IsBaby { get; set; }
		public bool IsConverting { get; set; }
		public bool IsCritical { get; set; }
		public bool IsShowName => !HideNameTag;
		public bool IsAlwaysShowName { get; set; }
		public bool IsNoAi => NoAi;
		public bool IsSilent { get; set; }
		public virtual bool IsWallClimbing { get; set; }
		public bool IsResting { get; set; }
		public bool IsSitting { get; set; }
		public bool IsAngry { get; set; }
		public bool IsInterested { get; set; }

		public bool IsTamed { get; set; }
		public bool IsLeashed { get; set; }
		public bool IsSheared { get; set; }
		public bool IsFlagAllFlying { get; set; }
		public bool IsElder { get; set; }
		public bool IsMoving { get; set; }
		public bool IsBreathing => !IsInWater;
		public virtual bool IsChested { get; set; }
		public bool IsStackable { get; set; }
		public bool HasCollision { get; set; } = true;
		public bool IsAffectedByGravity { get; set; } = true;
		
		public bool RenderEntity { get; set; } = true;
		public bool ShowItemInHand { get; set; } = false;

		internal bool RequiresRealTimeTick { get; set; } = true;

		internal bool HasPhysics { get; set; } = true;
		public void HandleJavaMetadata(MetaDataEntry entry)
		{
			if (entry.Index == 0 && entry is MetadataByte flags)
			{
				IsOnFire = (flags.Value & 0x01) != 0;
				IsSneaking = (flags.Value & 0x02) != 0;
				IsSprinting = (flags.Value & 0x08) != 0;
				IsInvisible = (flags.Value & 0x20) != 0;
			}
			else if (entry.Index == 2 && entry is MetadataOptChat customName)
			{
				if (customName.HasValue)
				{
					NameTag = customName.Value.RawMessage;
				}
			}
			else if (entry.Index == 3 && entry is MetadataBool showNametag)
			{
				HideNameTag = !showNametag.Value;
			}
			else if (entry.Index == 5 && entry is MetadataBool noGravity)
			{
				IsAffectedByGravity = !noGravity.Value;
			}
			else
			{
				HandleJavaMeta(entry);
			}
		}

		protected virtual void HandleJavaMeta(MetaDataEntry entry)
		{
			
		}
		
		public void HandleMetadata(MetadataDictionary metadata)
		{
			foreach (var meta in metadata._entries)
			{
				switch ((int) (MiNET.Entities.Entity.MetadataFlags) meta.Key)
				{
					case (int) MiNET.Entities.Entity.MetadataFlags.CollisionBoxHeight:
					{
						if (meta.Value is MetadataFloat flt)
						{
							Height = flt.Value;
						}
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.CollisionBoxWidth:
					{
						if (meta.Value is MetadataFloat fltw)
						{
							Width = fltw.Value;
						//	Length = fltw.Value;
						}
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.Scale:
					{
						if (meta.Value is MetadataFloat flt)
						{
							Scale = flt.Value;
						}
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.EntityFlags2:
					case (int) MiNET.Entities.Entity.MetadataFlags.EntityFlags:
					{
						if (meta.Value is MetadataLong lng)
						{
							HandleEntityFlags(lng.Value);
						}
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.NameTag:
					{
						if (meta.Value is MetadataString nametag)
						{
							NameTag = nametag.Value;
						}
					} break;

					case (int) MiNET.Entities.Entity.MetadataFlags.HideNameTag:
					{
						if (meta.Value is MiNET.Utils.MetadataByte hideNameTag)
						{
							//HideNameTag = hideNameTag.Value == 1;
						}
						break;
					}

					case (int) MiNET.Entities.Entity.MetadataFlags.MaxAir:
					{
						if (meta.Value is MiNET.Utils.MetadataShort airTag)
						{
							HealthManager.MaxAir = airTag.Value;
						}
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.AvailableAir:
					{
						if (meta.Value is MiNET.Utils.MetadataShort airTag)
						{
							HealthManager.AvailableAir = airTag.Value;
						}
					} break;

					default:
						if (!HandleMetadata((MiNET.Entities.Entity.MetadataFlags) meta.Key, meta.Value))
						{
							Log.Debug($"Unimplemented flag: {(MiNET.Entities.Entity.MetadataFlags) meta.Key}");
						}

						break;
				}
			}
		}

		protected virtual bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			return false;
		}

		private void HandleEntityFlags(long data)
		{
			if ((this is Player))
				return;
			
			IsInvisible = (data & ((int) MiNET.Entities.Entity.DataFlags.Invisible)) != 0;
			IsSneaking = (data & ((int) MiNET.Entities.Entity.DataFlags.Sneaking)) != 0;
			IsOnFire = (data & ((int) MiNET.Entities.Entity.DataFlags.OnFire)) != 0;
			IsSprinting = (data & ((int) MiNET.Entities.Entity.DataFlags.Sprinting)) != 0;
			
			NoAi = (data & ((int) MiNET.Entities.Entity.DataFlags.NoAi)) != 0;
			IsAffectedByGravity = (data & ((int) MiNET.Entities.Entity.DataFlags.AffectedByGravity)) != 0;
			//HasCollision = (data & ((int) MiNET.Entities.Entity.DataFlags.HasCollision)) != 0;
			
			IsAlwaysShowName = (data & ((int) MiNET.Entities.Entity.DataFlags.AlwaysShowName)) != 0;
			IsBaby = (data & ((int) MiNET.Entities.Entity.DataFlags.Baby)) != 0;
			IsUsingItem = (data & ((int) MiNET.Entities.Entity.DataFlags.UsingItem)) != 0;
			IsAngry = (data & ((int) MiNET.Entities.Entity.DataFlags.Angry)) != 0;
			IsInLove = (data & ((int) MiNET.Entities.Entity.DataFlags.InLove)) != 0;
			IsRiding = (data & ((int) MiNET.Entities.Entity.DataFlags.Riding)) != 0;
			IsTempted = (data & ((int) MiNET.Entities.Entity.DataFlags.Tempted)) != 0;
			IsTamed = (data & ((int) MiNET.Entities.Entity.DataFlags.Tamed)) != 0;
			IsLeashed = (data & ((int) MiNET.Entities.Entity.DataFlags.Leashed)) != 0;
			IsSheared = (data & ((int) MiNET.Entities.Entity.DataFlags.Sheared)) != 0;
			IsChested = (data & ((int) MiNET.Entities.Entity.DataFlags.Chested)) != 0;
			IsFlagAllFlying = (data & ((int) MiNET.Entities.Entity.DataFlags.FlagAllFlying)) != 0;
			IsSilent = (data & ((int) MiNET.Entities.Entity.DataFlags.Silent)) != 0;
			IsSitting = (data & ((int) MiNET.Entities.Entity.DataFlags.Sitting)) != 0;
			IsWallClimbing = (data & ((int) MiNET.Entities.Entity.DataFlags.WallClimbing)) != 0;
			IsResting = (data & ((int) MiNET.Entities.Entity.DataFlags.Resting)) != 0;
		}

		public virtual void Render(IRenderArgs renderArgs)
		{
			var  renderer = ModelRenderer;

			if (!IsInvisible && RenderEntity && renderer != null)
			{
				renderer.Render(renderArgs);
			}
			else if (ShowItemInHand && ItemRenderer != null && !_skipRendering)
			{
				ItemRenderer?.Render(renderArgs, null);
			}
		}

		public virtual void Update(IUpdateArgs args)
		{
			Movement?.Update(args.GameTime);
			
			var renderer = ModelRenderer;
			
			if (_head != null)
			{
				var headYaw = (RenderLocation.HeadYaw - RenderLocation.Yaw);

				var pitch = RenderLocation.Pitch;

				_head.Rotation = new Vector3(pitch, headYaw, 0f);
			}
			
			CalculateLegMovement(args);
			
			if (((!RenderEntity || IsInvisible) && !ShowItemInHand) || renderer == null || _skipRendering) return;

			renderer.Update(args, RenderLocation);

			//if (!ShowItemInHand || _skipRendering || ItemRenderer == null) return;
		}

		private long _hitAnimationEnd = 0;
		private bool _isHit = false;
		public void EntityHurt()
		{
			if (ModelRenderer == null)
				return;

			_isHit = true;
			_hitAnimationEnd = Age + 5;
			ModelRenderer.EntityColor = Color.Red.ToVector3();
		}
		
		protected bool DoRotationCalculations = true;

		private  float   _armRotation        = 0f;
		private  float   _legRotation        = 0f;
		
		public  float DistanceMoved { get; set; } = 0;
		private float _mvSpeed = 0f;

		public bool ServerEntity { get; protected set; } = true;

		public void SwingArm(bool broadcast = false)
		{
			bool isLeftHand = false;
			
			if (this is LivingEntity e)
			{
				if (e.IsLeftHanded)
				{
					isLeftHand = true;
				}
				else
				{
					isLeftHand = false;
				}
			}
			
			SwingArm(broadcast, isLeftHand);
		}
		
		public void SwingArm(bool broadcast, bool leftHanded)
		{
			EntityModelRenderer.ModelBone bone = leftHanded ? _leftArmModel : _rightArmModel;

			if (bone != null && (!bone.IsAnimating || bone.Animations.Count <= 1))
			{
				bone.Animations.Enqueue(new SwingAnimation(bone, TimeSpan.FromMilliseconds(200)));
			}

			if (broadcast)
			{
				Network.PlayerAnimate(leftHanded ? PlayerAnimations.SwingLeftArm : PlayerAnimations.SwingRightArm);
			}
		}
		
		private void CalculateLegMovement(IUpdateArgs args)
		{
			var   pos    = KnownPosition.ToVector3();

			var dt       = (float) args.GameTime.ElapsedGameTime.TotalSeconds;

			AnimateLegs(dt, _mvSpeed);
			Animate(dt, _mvSpeed);
		}

		protected virtual void AnimateLegs(float dt, float mvSpeed)
		{
			if (_leftLegModel != null && _rightLegModel != null)
			{
				Vector3 lLegRot = Vector3.Zero;
				Vector3 rLegRot = Vector3.Zero;

				if (mvSpeed > 0f)
				{
					_legRotation += (float) (_mvSpeed) * dt;
					
					lLegRot = new Vector3(MathF.Sin(_legRotation) * 34.5f, 0f, 0f);
					rLegRot = new Vector3(-MathF.Sin(_legRotation) * 34.5f, 0f, 0f);
				}
				else
				{
					_legRotation = 0f;
				}

				if (!_leftLegModel.IsAnimating)
					_leftLegModel.Rotation = lLegRot;
				
				if (!_rightLegModel.IsAnimating)
					_rightLegModel.Rotation = rLegRot;
			}
		}
		
		protected virtual void Animate(float dt, float mvSpeed)
		{
			if (IsSneaking && _body != null)
			{
				if (!_body.IsAnimating)
				{
					_body.Rotation = new Vector3(-25f, _body.Rotation.Y, _body.Rotation.Z);
				}
				
				if (_rightArmModel != null && _leftArmModel != null)
				{
					if (!_leftArmModel.IsAnimating)
					{
						_leftArmModel.Rotation = new Vector3(20f, 0f, 0f);
					}

					if (!_rightArmModel.IsAnimating)
					{
						_rightArmModel.Rotation = new Vector3(20f, 0f, 0f);
					}
				}
			}
			else if (_body != null && !IsSneaking)
			{
				if (!_body.IsAnimating)
				{
					_body.Rotation = new Vector3(0f);
				}
			}
			
			// Test arm rotations
			if (!IsSneaking && _leftArmModel != null && _rightArmModel != null)
			{
				Vector3 rArmRot;
				Vector3 lArmRot;
				
				if (_mvSpeed > 0f)
				{
					if (!IsMoving)
					{
						_armRotation = 0f;
						IsMoving = true;
					}

					_armRotation += (float) (_mvSpeed) * dt;
					//rArmRot = new Vector3(tcos0, 0, 0);
					rArmRot = new Vector3(MathF.Cos(_armRotation) * 24.5f, 0, 0);
					lArmRot = new Vector3(-MathF.Cos(_armRotation) * 24.5f, 0, 0);
				}
				else
				{
					IsMoving = false;
					_armRotation += dt;

					rArmRot = new Vector3(
						MathF.Cos(_armRotation) * 7.5f, 0f, 0.1f + (MathF.Sin(_armRotation) * -1.5f));
						
					lArmRot = new Vector3(
						-MathF.Cos(_armRotation) * 7.5f, 0f, 0.1f + (MathF.Sin(_armRotation) * -1.5f));
				}


				if (!_leftArmModel.IsAnimating)
				{
					_leftArmModel.Rotation = lArmRot;
				}

				if (!_rightArmModel.IsAnimating)
				{
					_rightArmModel.Rotation = rArmRot;
				}
			}
		}

		private DateTime _nextUpdate     = DateTime.MinValue;
		private DateTime _previousUpdate = DateTime.MinValue;
		public  float    CurrentSpeed => _mvSpeed;

		public virtual void OnTick()
		{
			Age++;

			Movement?.OnTick();

			HealthManager.OnTick();

			if (_isHit && Age > _hitAnimationEnd)
			{
				_isHit = false;
				ModelRenderer.EntityColor = Color.White.ToVector3();
			}

			if (DoRotationCalculations && !IsNoAi)
			{
				UpdateRotations();
			}
			else
			{
				KnownPosition.Yaw = KnownPosition.HeadYaw;
			}

			_previousPosition = KnownPosition;

			if (DateTime.UtcNow >= _nextUpdate)
			{
				var distanceMoved = DistanceMoved;
				DistanceMoved = 0;

				//PreviousUpdate
				_mvSpeed = (float) (distanceMoved * (TimeSpan.FromSeconds(1) / (DateTime.UtcNow - _previousUpdate)));

				_previousUpdate = DateTime.UtcNow;
				_nextUpdate = DateTime.UtcNow + TimeSpan.FromMilliseconds(500);
			}

			foreach (var effect in _effects.Values.ToArray())
			{
				effect.OnTick(this);
			}

			if (IsNoAi) return;
			//	IsMoving = Velocity.LengthSquared() > 0f;

			var knownPos  = new BlockCoordinates(new Vector3(KnownPosition.X, KnownPosition.Y, KnownPosition.Z));
			var knownDown = KnownPosition.GetCoordinates3D();

			//	if (Alex.ServerType == ServerType.Bedrock)
			{
				knownDown = knownDown.BlockDown();
			}

			var blockBelowFeet = Level?.GetBlockStates(knownDown.X, knownDown.Y, knownDown.Z);
			var feetBlock      = Level?.GetBlockStates(knownPos.X, knownPos.Y, knownPos.Z).ToArray();
			var headBlockState = Level?.GetBlockState(KnownPosition.GetCoordinates3D() + new BlockCoordinates(0, 1, 0));

			if (headBlockState != null)
			{
				var headBlock = headBlockState.Block;

				if (headBlock.Solid)
				{
					HeadInBlock = true;
				}
				else
				{
					HeadInBlock = false;
				}

				if (headBlock.BlockMaterial == Material.Water || headBlock.IsWater)
				{
					HeadInWater = true;
				}
				else
				{
					HeadInWater = false;
				}

				if (headBlock.BlockMaterial == Material.Lava || headBlock is Lava || headBlock is FlowingLava)
				{
					HeadInLava = true;
				}
				else
				{
					HeadInLava = false;
				}
			}

			if (blockBelowFeet != null)
			{
				if (blockBelowFeet.Any(b => b.State.Block.BlockMaterial == Material.Water || b.State.Block.IsWater))
				{
					AboveWater = true;
				}
				else
				{
					AboveWater = false;
				}
			}
			else
			{
				AboveWater = false;
			}

			if (feetBlock != null)
			{
				if (feetBlock.Any(b => b.State.Block.BlockMaterial == Material.Water || b.State.Block.IsWater))
				{
					FeetInWater = true;
				}
				else
				{
					FeetInWater = false;
				}

				if (feetBlock.Any(b => b.State.Block.BlockMaterial == Material.Lava))
				{
					FeetInLava = true;
				}
				else
				{
					FeetInLava = false;
				}
			}

			IsInWater = FeetInWater || HeadInWater;
			IsInLava = FeetInLava || HeadInLava;
			//HealthManager.OnTick();
		}

		private int _turnTicks;
		private int turnTicksLimit = 10;
		private float _lastRotationYawHead = 0f;
		private Vector3 _previousPosition = Vector3.Zero;
		protected bool SnapHeadYawRotationOnMovement { get; set; } = true;
		protected bool SnapYawRotationOnMovement { get; set; } = false;
		
		private void UpdateRotations()
		{
			double deltaX = KnownPosition.X - _previousPosition.X;
			double deltaZ = KnownPosition.Z - _previousPosition.Z;
			double distSQ = deltaX * deltaX + deltaZ * deltaZ;

			IsMoving = distSQ > 0f || Velocity.LengthSquared() > 0f;

			float maximumHeadBodyAngleDifference = 75f;
			const float movementThresholdSq = 2.5f;
			// if moving:
			// 1) snap the body yaw (renderYawOffset) to the movement direction (rotationYaw)
			// 2) constrain the head yaw (rotationYawHead) to be within +/- 90 of the body yaw (renderYawOffset)
			if (distSQ > movementThresholdSq)
			{
				//dragon.renderYawOffset = dragon.rotationYaw;
				float newRotationYawHead = MathUtils.ConstrainAngle(KnownPosition.Yaw, KnownPosition.HeadYaw,
					maximumHeadBodyAngleDifference);
				
				if (SnapHeadYawRotationOnMovement)
				{
					KnownPosition.HeadYaw = newRotationYawHead;
				}
				
				if (SnapYawRotationOnMovement)
				{
					KnownPosition.Yaw = KnownPosition.HeadYaw;
				}

				_lastRotationYawHead = newRotationYawHead;
				_turnTicks = 0;
			}
			else
			{
				var changeInHeadYaw = Math.Abs(KnownPosition.HeadYaw - _lastRotationYawHead);
				if (changeInHeadYaw > 15f)
				{
					_turnTicks = 0;
					_lastRotationYawHead = KnownPosition.HeadYaw;
				}
				else
				{
					_turnTicks++;
					if (_turnTicks > turnTicksLimit)
					{
						maximumHeadBodyAngleDifference =
							Math.Max(1f - (float) ((float)(_turnTicks - turnTicksLimit) / turnTicksLimit), 0f) *
							maximumHeadBodyAngleDifference;
					}
				}

				KnownPosition.Yaw = MathUtils.ConstrainAngle(KnownPosition.Yaw, KnownPosition.HeadYaw,
					maximumHeadBodyAngleDifference);
			}
		}

		protected virtual void UpdateModelParts()
		{
			if (ModelRenderer == null)
				return;

			ScaleChanged();
			
			ModelRenderer.GetBone("body", out _body);

			ModelRenderer.GetBone("leftArm", out _leftArmModel);
			ModelRenderer.GetBone("leftItem", out _leftItemModel);
			ModelRenderer.GetBone("rightArm", out _rightArmModel);
			ModelRenderer.GetBone("rightItem", out _rightItemModel);

			ModelRenderer.GetBone("rightLeg", out _rightLegModel);
			ModelRenderer.GetBone("leftLeg", out _leftLegModel);

			ModelRenderer.GetBone("head", out _head);
		}

		protected virtual void OnModelUpdated()
		{
			
		}

		private BoundingBox _boundingBox = new BoundingBox();
		public  BoundingBox BoundingBox => _boundingBox;
		public virtual BoundingBox GetBoundingBox()
		{
			return _boundingBox;
			//var pos = KnownPosition;
			//return GetBoundingBox(pos);
		}

		public virtual BoundingBox GetBoundingBox(Vector3 pos)
		{
			var    width     = Width;
			var    height    = Height;
			
			double halfWidth = (width * Scale) / 2D;
			double halfDepth = (width * Scale) / 2D;

			return new BoundingBox(
				new Vector3((float) (pos.X - halfWidth), pos.Y, (float) (pos.Z - halfDepth)),
				new Vector3(
					(float) (pos.X + halfWidth), (float) (pos.Y + (height * Scale)), (float) (pos.Z + halfDepth)));
		}

		public virtual BoundingBox GetVisibilityBoundingBox(Vector3 pos)
		{
			var width  = Width;
			var height = Height;

			if (ModelRenderer?.Model != null)
			{
				width = ModelRenderer.Model.Description.VisibleBoundsWidth;
				height = ModelRenderer.Model.Description.VisibleBoundsHeight;
			}

			double halfWidth = (width * Scale) / 2D;
			double halfDepth = (width * Scale) / 2D;

			return new BoundingBox(
				new Vector3((float) (pos.X - halfWidth), pos.Y, (float) (pos.Z - halfDepth)),
				new Vector3(
					(float) (pos.X + halfWidth), (float) (pos.Y + (height * Scale)), (float) (pos.Z + halfDepth)));
		}

		public bool IsColliding(Entity other)
		{
			return IsColliding(GetBoundingBox(), other);
		}

		public bool IsColliding(BoundingBox bbox, Entity other)
		{
			//if (!Compare((int) KnownPosition.X, (int) other.KnownPosition.X, 5)) return false;
			//if (!Compare((int) KnownPosition.Z, (int) other.KnownPosition.Z, 5)) return false;
			if (!Compare((int) KnownPosition.X, (int) other.KnownPosition.X, 4)) return false;
			if (!Compare((int) KnownPosition.Z, (int) other.KnownPosition.Z, 4)) return false;
			if (!bbox.Intersects(other.GetBoundingBox())) return false;

			return true;
		}

		private bool Compare(int a, int b, int m)
		{
			a = a >> m;
			b = b >> m;
			return a == b || a == b - 1 || a == b + 1;
		}

		public virtual void CollidedWithWorld(Vector3 direction, Vector3 position, float impactVelocity)
		{
			if (direction == Vector3.Down)
			{
				//Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
				KnownPosition.OnGround = true;
			}
			else if (direction == Vector3.Left || direction == Vector3.Right)
			{
			//	Velocity = new Vector3(0, Velocity.Y, Velocity.Z);
			}
			else if (direction == Vector3.Forward || direction == Vector3.Backward)
			{
			//	Velocity = new Vector3(Velocity.X, Velocity.Y, 0);
			}
		}

		protected bool TryUpdateGeometry(ResourceLocation location, string geometry, string texture = "default")
		{
			if (Alex.Instance.Resources.BedrockResourcePack.EntityDefinitions.TryGetValue(
				location, out var entityDescription))
			{
				if (entityDescription.Textures.TryGetValue(texture, out texture)
				    && entityDescription.Geometry.TryGetValue(geometry, out var geometryName))
				{
					if (ModelFactory.TryGetModel(geometryName, out var newModel))
					{

						if (Alex.Instance.Resources.BedrockResourcePack.TryGetTexture(texture, out var newTexture))
						{
							ModelRenderer = new EntityModelRenderer(
								newModel, TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, newTexture));

							//ModelRenderer?.Texture = TextureUtils.BitmapToTexture2D(
							//	Alex.Instance.GraphicsDevice, newTexture);
						}
					}

					return true;
				}
			}


			return false;
		}

		protected bool TryUpdateTexture(ResourceLocation location, string texture)
		{
			if (Alex.Instance.Resources.BedrockResourcePack.EntityDefinitions.TryGetValue(
				location, out var entityDescription))
			{
				if (entityDescription.Textures.TryGetValue(texture, out texture))
				{
					if (Alex.Instance.Resources.BedrockResourcePack.TryGetTexture(texture, out var newTexture))
					{
						ModelRenderer.Texture = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, newTexture);
						return true;
					}
				}
			}

			return false;
		}
		
		protected void ToggleCubes(EntityModelRenderer.ModelBone bone, bool isInvisible)
		{
			bone.Rendered = !isInvisible;
		}

		public void Dispose()
		{
			var model = ModelRenderer;
			ModelRenderer = null;
			model?.Dispose();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return base.ToString();
		}

		private ConcurrentDictionary<EffectType, Effect> _effects = new ConcurrentDictionary<EffectType, Effect>();
		private IItemRenderer                            _itemRenderer    = null;

		public const float   JumpVelocity = 0.42f;
		public void Jump()
		{
			HealthManager.Exhaust(IsSprinting ? 0.2f : 0.05f);
			var jumpVelocity = JumpVelocity;
			
			if (IsInWater)
			{
				jumpVelocity = 0.04f;
				
				if (FeetInWater && !HeadInWater)
				{
					jumpVelocity += 0.3f;
				}
			}
			else if (IsInLava)
			{
				jumpVelocity = 0.04f;
				
				if (FeetInLava && !HeadInLava)
				{
					jumpVelocity += 0.3f;
				}
			}
			else
			{
				if (_effects.TryGetValue(EffectType.JumpBoost, out var effect))
				{
					jumpVelocity += ((jumpVelocity * 0.5f) * effect.Level);
				}
			}

			//Movement.Move(new Vector3(0f, jumpVelocity, 0f));
			Velocity += new Vector3(0f, jumpVelocity, 0f);
			//Velocity += new Vector3(0f, MathF.Sqrt(2f * (float) (Gravity * 20f) * 1.2f), 0f);
			Network?.EntityAction((int) EntityId, EntityAction.Jump);
		}
		
		public void AddOrUpdateEffect(Effect effect)
		{
			var effect1 = effect;
			
			effect = _effects.AddOrUpdate(effect.EffectId, effect, (type, e) => effect1);
			effect?.ApplyTo(this);
		}

		public void RemoveEffect(EffectType effectType)
		{
			if (_effects.TryRemove(effectType, out var removed))
			{
				removed.TakeFrom(this);
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

		public virtual void OnSpawn()
		{
			IsSpawned = true;
		}

		public virtual void OnDespawn()
		{
			IsSpawned = false;
		}
	}
}