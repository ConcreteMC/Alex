using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.API.Data.Servers;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.API.Utils.Collections;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Entities.Components;
using Alex.Entities.Effects;
using Alex.Entities.Properties;
using Alex.Gamestates;
using Alex.Graphics.Effect;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.Graphics.Models.Items;
using Alex.Items;
using Alex.MoLang.Attributes;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Alex.Utils.Inventories;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Java;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Utils;
using MiNET.Utils.Metadata;
using Mono.Collections.Generic;
using NLog;
using BlockCoordinates = Alex.API.Utils.Vectors.BlockCoordinates;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using Effect = Alex.Entities.Effects.Effect;
using HealthManager = Alex.Entities.Meta.HealthManager;
using Inventory = Alex.Utils.Inventories.Inventory;
using MathF = System.MathF;
using MetadataByte = Alex.Networking.Java.Packets.Play.MetadataByte;
using MetadataFloat = Alex.Networking.Java.Packets.Play.MetadataFloat;
using MetadataString = Alex.Networking.Java.Packets.Play.MetadataString;
using ModelBone = Alex.Graphics.Models.Entity.ModelBone;
using PlayerLocation = Alex.API.Utils.Vectors.PlayerLocation;
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
				var oldValue = _modelRenderer;

				try
				{
					ItemRenderer = null;
					_modelRenderer = value;

					if (value != null)
					{
						value.Use();
						UpdateModelParts();
						OnModelUpdated();
						CheckHeldItem();
					}

					AnimationController.Enabled = value != null;
				}
				finally
				{
					oldValue?.Dispose();
				}
			}
		}
		
		public EntityMovement Movement { get; }

		public World Level { get; set; }

		public long EntityId { get; internal set; } = -1;
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
				return _renderLocation;
			}
			set
			{
				_renderLocation = value;
			}
		}

		public float   Slipperines { get; set; } = 0.6f;
		public Vector3 Velocity    { get; set; } = Vector3.Zero;

		//public HealthManager HealthManager { get; set; }
		public string NameTag
		{
			get => _nameTag;
			set
			{
				_nameTag = value;
				_nameTagLines = null;
			}
		}

		private string[] _nameTagLines = null;
		internal string[] NameTagLines
		{
			get
			{
				if (_nameTagLines == null && _nameTag != null)
				{
					_nameTagLines = _nameTag.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).Reverse().Select(x => x.Trim()).ToArray();
				}

				return _nameTagLines;
			}
		}

		public virtual bool NoAi
		{
			get => _noAi;
			set
			{
				_noAi = value;
				
				if (_noAi)
					Velocity = Vector3.Zero;
			}
		}

		public bool HideNameTag { get; set; } = false;
		
		public bool HeadInBlock  { get; set; } = false;
		public bool AboveWater   { get; set; } = false;
		public bool HeadInWater  { get; set; } = false;
		public bool FeetInWater  { get; set; } = false;
		
		[MoProperty("is_in_water")]
		public bool IsInWater    { get; set; } = false;

		[MoProperty("is_in_water_or_rain")]
		public bool IsInWaterOrRain => IsInWater || Level.Raining;
		
		public bool HeadInLava  { get; set; } = false;
		public bool FeetInLava  { get; set; } = false;
		public bool IsInLava     { get; set; } = false;
		public bool IsOutOfWater => !IsInWater;
		public bool Invulnerable { get; set; } = false;

		//public long Age { get; set; }

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

		[MoProperty("can_fly")]
		public bool CanFly { get; set; } = false;
		public bool IsFlying { get; set; } = false;

		private bool _isRendered = false;

		public bool IsWorldImmutable { get; set; } = false;
		public bool IsNoPvP { get; set; } = true;
		public bool IsNoPvM { get; set; } = true;
		
		[MoProperty("is_on_screen")]
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

				if (value && !oldValue)
				{
					Velocity = Vector3.Zero;
				}
			}
		}
		
		public Inventory Inventory { get; protected set; }

		public IItemRenderer ItemRenderer
		{
			get => _itemRenderer;
			private set
			{
				var oldValue = _itemRenderer;
				var newValue = value;
				//_itemRenderer = value;

				oldValue?.Parent?.Remove(oldValue);

				if (newValue != null)
				{
					IAttached arm = null;

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
				
				UpdateItemPosition(newValue);
			}
		}

		protected ModelBone _leftArmModel;
		protected ModelBone _leftItemModel;
		protected ModelBone _rightArmModel;
		protected ModelBone _rightItemModel;

		protected ModelBone _head;
		
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
					UpdateItemPosition(_itemRenderer);
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
		
		public AnimationController AnimationController { get; }
		public TimeSpan LifeTime => DateTime.UtcNow - TimeOfCreation;
		private readonly DateTime TimeOfCreation;
		private Stopwatch _lifeTime;

		public float Experience { get; set; } = 0;
		public float ExperienceLevel { get; set; } = 0; 
		protected ConcurrentStack<IEntityComponent> EntityComponents { get; }
		public Entity(World level)
		{
			TimeOfCreation = DateTime.UtcNow;
			EntityComponents = new ConcurrentStack<IEntityComponent>();
			
			_lifeTime = Stopwatch.StartNew();
			
			EntityId = -1;
            Level = level;
            KnownPosition = new PlayerLocation();
            RenderLocation = new PlayerLocation();
            //Inventory = new Inventory(46);
            SetInventory(new Inventory(46));
            //	HealthManager = new HealthManager(this);

            HideNameTag = false;
			//ServerEntity = true;
			IsAffectedByGravity = true;
			//HasPhysics = true;
			
			HealthManager = new HealthManager(this);
			UUID = new MiNET.Utils.UUID(Guid.NewGuid().ToByteArray());
			
			//BaseMovementSpeed = 4.317D;
			
			AddOrUpdateProperty(new FlyingSpeedProperty(this));
			AddOrUpdateProperty(new MovementSpeedProperty(this));
			Movement = new EntityMovement(this);
				
			//EntityComponents.Push(Movement = new EntityMovement(this));
			EntityComponents.Push(AnimationController = new AnimationController(this));
			
			Effect = new EntityEffect();
			Effect.Texture = _texture;
			Effect.VertexColorEnabled = true;
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
	        if (_isUsingItem && Inventory.SelectedSlot != _usingSlot)
	        {
		        IsUsingItem = false;
	        }
	        
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

        protected virtual void UpdateItemPosition(IItemRenderer renderer)
        {
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

		public double AttackTime => -1d;
		
		[MoProperty("is_onfire"), MoProperty("is_on_fire")]
		public bool IsOnFire { get; set; } = false;
		
		[MoProperty("is_sneaking")]
		public bool IsSneaking { get; set; }
		
		[MoProperty("is_riding")]
		public bool IsRiding { get; set; }
		
		[MoProperty("is_sprinting")]
		public bool IsSprinting { get; set; }

		private DateTime _startOfItemUse = DateTime.UtcNow;
		private int _usingSlot = 0;
		[MoProperty("is_using_item")]
		public bool IsUsingItem
		{
			get => _isUsingItem;
			set
			{
				if (_isUsingItem != value)
				{
					//Log.Info($"Using Item={value}, duration={(DateTime.UtcNow - _startOfItemUse).TotalMilliseconds}ms");
				}
				
				_isUsingItem = value;

				if (value)
				{
					_usingSlot = Inventory.SelectedSlot;
					_startOfItemUse = DateTime.UtcNow;
				}
			}
		}

		[MoProperty("item_in_use_duration")]
		public double ItemInUseDuration
		{
			get
			{
				if (_isUsingItem)
				{
					return (DateTime.UtcNow - _startOfItemUse).TotalSeconds;
				}

				return 0d;
			}
		}

		public bool IsInvisible { get; set; }
		
		[MoProperty("is_tempted")]
		public bool IsTempted { get; set; }
		
		[MoProperty("is_in_love")]
		public bool IsInLove { get; set; }
		
		[MoProperty("is_powered")]
		public bool IsPowered { get; set; }
		
		[MoProperty("is_ignited")]
		public bool IsIgnited { get; set; }
		
		[MoProperty("is_baby")]
		public bool IsBaby { get; set; }
		
		[MoProperty("is_converting")]
		public bool IsConverting { get; set; }
		
		[MoProperty("is_critical")]
		public bool IsCritical { get; set; }
		public bool IsShowName => !HideNameTag;
		public bool IsAlwaysShowName { get; set; }
		
		[MoProperty("is_silent")]
		public bool IsSilent { get; set; }
		
		[MoProperty("is_wall_climbing")]
		public virtual bool IsWallClimbing { get; set; }
		
		[MoProperty("is_resting")]
		public bool IsResting { get; set; }
		
		[MoProperty("is_sitting")]
		public bool IsSitting { get; set; }
		
		[MoProperty("is_charging")]
		public bool IsAngry { get; set; }
		
		[MoProperty("is_interested")]
		public bool IsInterested { get; set; }

		[MoProperty("is_tamed")]
		public bool IsTamed { get; set; }
		
		[MoProperty("is_leashed")]
		public bool IsLeashed { get; set; }
		
		[MoProperty("is_sheared")]
		public bool IsSheared { get; set; }
		public bool IsFlagAllFlying { get; set; }
		
		[MoProperty("is_elder")]
		public bool IsElder { get; set; }
		
		[MoProperty("is_moving")]
		public bool IsMoving { get; set; }
		
		[MoProperty("is_breathing")]
		public bool IsBreathing => !IsInWater;
		
		[MoProperty("is_chested")]
		public virtual bool IsChested { get; set; }
		
		[MoProperty("is_stackable")]
		public bool IsStackable { get; set; }
		
		[MoProperty("is_eating")]
		public bool IsEating { get; set; }
		
		[MoProperty("blocking"), MoProperty("is_blocking")]
		public bool IsBlocking { get; set; }
		public bool IsSpinAttacking { get; set; }
		
		[MoProperty("has_collision")]
		public bool HasCollision { get; set; } = true;
		
		[MoProperty("has_gravity")]
		public bool IsAffectedByGravity { get; set; } = true;
		
		[MoProperty("is_swimming")]
		public virtual bool IsSwimming { get; set; }
		
		[MoProperty("is_sleeping")]
		public bool IsSleeping { get; set; }
		
		[MoProperty("is_standing")]
		public bool IsStanding { get; set; } = true;
		
		[MoProperty("is_emoting")]
		public bool IsEmoting { get; set; } = false;
		
		[MoProperty("is_gliding")]
		public bool IsGliding { get; set; } = false;
		
		[MoProperty("is_levitating")]
		public bool IsLevitating { get; set; } = false;

		[MoProperty("is_alive")]
		public bool IsAlive => HealthManager.Health > 0;

		[MoProperty("is_on_ground")]
		public bool IsOnGround => _knownPosition.OnGround;

		[MoProperty("anim_time")]
		public double AnimationTime => GetLifeTime();
		
		[MoProperty("is_grazing")]
		public bool IsGrazing { get; set; } = false;

		[MoProperty("key_frame_lerp_time")]
		public double KeyframeLerpTime
		{
			get;
			set;
		}

		[MoProperty("swell_amount")]
		public double SwellAmount { get; set; } = 0d;

		[MoProperty("invulnerable_ticks")]
		public double InvulnerableTicks { get; set; } = 0d;
		
		public Pose Pose { get; set; } = Pose.Standing;
		
		public bool RenderEntity { get; set; } = true;
		public bool ShowItemInHand { get; set; } = false;
		public long TargetEntityId { get; set; } = -1;
		public long OwnerEntityId { get; set; } = -1;
		public Vector2 TargetRotation { get; private set; } = Vector2.Zero;

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
					NameTag = customName.Value;
				}
			}
			else if (entry.Index == 3 && entry is MetadataBool showNametag)
			{
				HideNameTag = !showNametag.Value;
			}
			else if (entry.Index == 4 && entry is MetadataBool sil)
			{
				IsSilent = sil.Value;
			}
			else if (entry.Index == 5 && entry is MetadataBool noGravity)
			{
				IsAffectedByGravity = !noGravity.Value;
			}
			else if (entry.Index == 6 && entry is MetadataPose pose)
			{
				Pose = pose.Value;

				switch (Pose)
				{
					case Pose.Standing:
						IsSneaking = false;
						IsStanding = true;
						IsSleeping = false;
						IsSwimming = false;
						IsSpinAttacking = false;
						break;
					
					case Pose.Sleeping:
						IsSneaking = false;
						IsStanding = false;
						IsSleeping = true;
						IsSwimming = false;
						IsSpinAttacking = false;
						break;

					case Pose.Swimming:
						IsSneaking = false;
						IsStanding = false;
						IsSleeping = false;
						IsSwimming = true;
						IsSpinAttacking = false;
						break;

					case Pose.SpinAttack:
						IsSneaking = false;
						IsStanding = false;
						IsSleeping = false;
						IsSwimming = false;
						IsSpinAttacking = true;
						break;

					case Pose.Sneaking:
						IsSneaking = true;
						IsStanding = false;
						IsSleeping = false;
						IsSwimming = false;
						IsSpinAttacking = false;
						break;

					case Pose.Dying:
						break;
					
					case Pose.FallFlying:
						break;
				}
			}
			else
			{
				HandleJavaMeta(entry);
			}
		}

		protected virtual void HandleJavaMeta(MetaDataEntry entry)
		{
			
		}
		
		public Vector3 RiderSeatPosition { get; set; } = Vector3.Zero;
		public float RiderMinRotation { get; set; } = 0f;
		public float RiderMaxRotation { get; set; } = 360f;
		public bool RiderRotationLocked { get; set; } = true;
		public void HandleMetadata(MetadataDictionary metadata)
		{
			foreach (var meta in metadata._entries)
			{
				switch ((int) (MiNET.Entities.Entity.MetadataFlags) meta.Key)
				{
					case (int) MiNET.Entities.Entity.MetadataFlags.CollisionBoxHeight:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataFloat flt)
						{
							Height = flt.Value;
						}
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.CollisionBoxWidth:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataFloat fltw)
						{
							Width = fltw.Value;
						//	Length = fltw.Value;
						}
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.Scale:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataFloat flt)
						{
							Scale = flt.Value;
						}
					} break;

					case (int) MiNET.Entities.Entity.MetadataFlags.RiderSeatPosition:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataVector3 seatPosition)
						{
							var val = seatPosition.Value;
							RiderSeatPosition = new Vector3(val.X, val.Y, val.Z);
						}	
					} break;

					case (int) MiNET.Entities.Entity.MetadataFlags.RiderMinRotation:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataFloat mdf)
						{
							RiderMinRotation = mdf.Value;
						}	
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.RiderMaxRotation:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataFloat mdf)
						{
							RiderMaxRotation = mdf.Value;
						}	
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.RiderRotationLocked:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataByte mdf)
						{
							RiderRotationLocked = mdf.Value == 1;
						}	
					} break;
					
					//case (int) MiNET.Entities.Entity.MetadataFlags.EntityFlags2:
					case 92://((int) MiNET.Entities.Entity.MetadataFlags.EntityFlags2):
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataLong lng)
						{
							_extendedData = lng.Value;
							HandleEntityFlags(_data, _extendedData);
						}
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.EntityFlags:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataLong lng)
						{
							_data = lng.Value;
							HandleEntityFlags(_data, _extendedData);
						}
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.NameTag:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataString nametag)
						{
							NameTag = nametag.Value;
						}
					} break;

					/*case (int) MiNET.Entities.Entity.MetadataFlags.HideNameTag:
					{
						if (meta.Value is MiNET.Utils.MetadataByte hideNameTag)
						{
							HideNameTag = hideNameTag.Value == 0;
						}
						break;
					}*/

					case (int) MiNET.Entities.Entity.MetadataFlags.MaxAir:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataShort airTag)
						{
							HealthManager.MaxAir = airTag.Value;
						}
					} break;
					
					case (int) MiNET.Entities.Entity.MetadataFlags.AvailableAir:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataShort airTag)
						{
							HealthManager.AvailableAir = airTag.Value;
						}
					} break;

					case 5: //Owner Entity ID
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataLong targetTag)
						{
							OwnerEntityId = targetTag.Value;
						}
					} break;
					
					case 6: //Target Entity ID
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataLong targetTag)
						{
							TargetEntityId = targetTag.Value;
						}
					} break;

					case 81:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataByte alwaysShowNameTag)
						{
							IsAlwaysShowName = alwaysShowNameTag.Value == 1;
						}	
					} break;

					default:
						if (!HandleMetadata((MiNET.Entities.Entity.MetadataFlags) meta.Key, meta.Value))
						{
							Log.Debug($"({GetType().Name}) Unimplemented flag: {(MiNET.Entities.Entity.MetadataFlags) meta.Key}");
						}

						break;
				}
			}
		}

		protected virtual bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			return false;
		}

		public virtual void HandleEntityStatus(byte status)
		{
			
		}

		private DateTime _attackStart = DateTime.UtcNow;
		public bool IsAttacking
		{
			get => _isAttacking;
			set
			{
				var oldValue = _isAttacking;
				
				if (!oldValue && value)
				{
					_attackStart = DateTime.UtcNow;
					_isAttacking = true;
				}
				else if (oldValue && !value)
				{
					_isAttacking = false;
				}
			}
		}

		private long _eatingEndTime = 0;
		private long _grazingEndTime = 0;
		public virtual void HandleEntityEvent(byte eventId, int data)
		{
			if (eventId == 1) //Entity Jump
			{
				Jump();
			}
			else if (eventId == 2) //Entity Hurt
			{
				EntityHurt();
			}
			else if (eventId == 3) //Entity Death
			{
				EntityDied();
			}
			else if (eventId == 4) //Arm Swing
			{
				SwingArm();
				//IsAttacking = true;
			}
			else if (eventId == 5) //Stop Attack
			{
				//SwingArm();
				IsAttacking = false;
			}
			/*else if (eventId == 9) //Use Item
			{
				IsUsingItem = true;
				Level.Ticker.ScheduleTick(
					() =>
					{
						IsUsingItem = false;
					}, 40, CancellationToken.None);
			}*/
			else if (eventId == 10) //Eat block
			{
				IsGrazing = true;
				_grazingEndTime = Age + 40;
				
			//	Log.Info($"Grazing: {IsGrazing} | {data}");
			}
			else if (eventId == 57) //Eat item
			{
				IsEating = true;
				_eatingEndTime = Age + 40;
				
			//	Log.Info($"Eating: {IsEating} | {data}");
			}
			else
			{
				Log.Warn($"{GetType().ToString()} unknown event. Id={eventId} data={data}");
			}
		}

		private long _extendedData = 0;
		private long _data = 0;
		
		private void HandleEntityFlags(long rawData, long extendedData)
		{
			if ((this is Player))
				return;

			BitArray data = new BitArray(
				BitConverter.GetBytes(rawData).Concat(BitConverter.GetBytes(extendedData)).ToArray());
			
			IsInvisible = data[(int) MiNET.Entities.Entity.DataFlags.Invisible];// (data & ((int) MiNET.Entities.Entity.DataFlags.Invisible)) != 0;
			IsSneaking = data[(int) MiNET.Entities.Entity.DataFlags.Sneaking];
			IsSitting = data[(int) MiNET.Entities.Entity.DataFlags.Sitting];
			
			IsOnFire = data[(int) MiNET.Entities.Entity.DataFlags.OnFire];
			IsSprinting = data[(int) MiNET.Entities.Entity.DataFlags.Sprinting];
			
			NoAi = data[(int) MiNET.Entities.Entity.DataFlags.NoAi];
			IsAffectedByGravity = data[(int) MiNET.Entities.Entity.DataFlags.AffectedByGravity];
			HasCollision = data[(int) MiNET.Entities.Entity.DataFlags.HasCollision];
			
			//HideNameTag = !data[(int) MiNET.Entities.Entity.DataFlags.ShowName];
			IsAlwaysShowName = data[(int) MiNET.Entities.Entity.DataFlags.AlwaysShowName];
			IsBaby = data[(int) MiNET.Entities.Entity.DataFlags.Baby];
			IsUsingItem = data[(int) MiNET.Entities.Entity.DataFlags.UsingItem];
			IsAngry = data[(int) MiNET.Entities.Entity.DataFlags.Angry];
			IsInLove = data[(int) MiNET.Entities.Entity.DataFlags.InLove];
			IsRiding = data[(int) MiNET.Entities.Entity.DataFlags.Riding];
			IsTempted = data[(int) MiNET.Entities.Entity.DataFlags.Tempted];
			IsTamed = data[(int) MiNET.Entities.Entity.DataFlags.Tamed];
			IsLeashed = data[(int) MiNET.Entities.Entity.DataFlags.Leashed];
			IsSheared = data[(int) MiNET.Entities.Entity.DataFlags.Sheared];
			IsChested = data[(int) MiNET.Entities.Entity.DataFlags.Chested];
			IsFlagAllFlying = data[(int) MiNET.Entities.Entity.DataFlags.FlagAllFlying];
			IsSilent = data[(int) MiNET.Entities.Entity.DataFlags.Silent];
			IsWallClimbing = data[(int) MiNET.Entities.Entity.DataFlags.WallClimbing];
			IsResting = data[(int) MiNET.Entities.Entity.DataFlags.Resting];
			IsMoving = data[(int) MiNET.Entities.Entity.DataFlags.Moving];
			IsElder = data[(int) MiNET.Entities.Entity.DataFlags.Elder];
			IsEating = data[62];
			IsSleeping = data[75];
			IsBlocking = data[71];
			IsSpinAttacking = data[55];
			IsSwimming = data[56];
			//IsFlying = data[(int) MiNET.Entities.Entity.DataFlags.fl]
		}

		private PooledTexture2D _texture;

		public PooledTexture2D Texture
		{
			get
			{
				return _texture;
			}
			set
			{
				if (value == _texture)
					return;
				
				PooledTexture2D oldValue = _texture;

				try
				{
					_texture = value;
					value?.Use(this);
					
					if (Effect != null && value != null)
					{
						Effect.Texture = value;
					}
				}
				finally
				{
					oldValue?.Release(this);
					oldValue?.ReturnResource(this);
				}
			}
		}
		
		private EntityEffect    Effect       { get; set; }
		
		/// <summary>
		///		Renders the entity
		/// </summary>
		/// <param name="renderArgs"></param>
		/// <returns>The amount of draw calls made</returns>
		public virtual int Render(IRenderArgs renderArgs, bool useCulling)
		{
			int renderCount = 0;
			var  renderer = ModelRenderer;

			if (!IsInvisible && RenderEntity && renderer != null)
			{
				renderCount += renderer.Render(renderArgs, useCulling, Effect, Matrix.CreateScale(Scale / 16f) * RenderLocation.CalculateWorldMatrix());
			}
			else if (ShowItemInHand && ItemRenderer != null && !_skipRendering)
			{
				renderCount += ItemRenderer.Render(renderArgs, null, Matrix.CreateScale(Scale / 16f) * RenderLocation.CalculateWorldMatrix());
			}

			return renderCount;
		}

		private bool _doUseItemAnimation = false;
		public virtual void Update(IUpdateArgs args)
		{
			var elapsed = args.GameTime.ElapsedGameTime.TotalSeconds;

			if (_doUseItemAnimation)
			{
				if (UseItemStartupProgress < 1f)
				{
					UseItemStartupProgress += elapsed * 4;
				}

				if (UseItemIntervalProgress < 1f)
				{
					UseItemIntervalProgress += elapsed * 4;
				}
				else
				{
					_doUseItemAnimation = false;
					UseItemStartupProgress = 0d;
					UseItemIntervalProgress = 0d;
				}
				//else if (UseItemIntervalProgress < 1f)
				//{
				//	UseItemIntervalProgress += elapsed * 3;
				//}
				//else
				//{
					
				//}
			}

		/*	if (UseItemIntervalProgress > 0d)
			{
				UseItemIntervalProgress -= elapsed;
			}
			
			if (UseItemStartupProgress > 0d)
			{
				UseItemStartupProgress -= elapsed;
			}*/

			foreach (var entityComponent in EntityComponents)
			{
				entityComponent.Update(args.GameTime);
			}

			Movement?.Update(args.GameTime);

			var renderer = ModelRenderer;

			if (renderer == null || _skipRendering)
				return;
			//if (((!RenderEntity || IsInvisible) && !ShowItemInHand) || renderer == null || _skipRendering) return;

			if (_head != null)
			{
				var headYaw = (KnownPosition.HeadYaw - KnownPosition.Yaw);

				var pitch = KnownPosition.Pitch;

				_head.Rotation = new Vector3(pitch, headYaw, 0f);
			}
			
			Effect.View = args.Camera.ViewMatrix;
			Effect.Projection = args.Camera.ProjectionMatrix;
			Effect.DiffuseColor = renderer.EntityColor * renderer.DiffuseColor;
			Effect.Texture = _texture;
			renderer.Update(args);

			//if (!ShowItemInHand || _skipRendering || ItemRenderer == null) return;
		}
		
		public virtual void EntityHurt()
		{
			
		}

		public virtual void EntityDied()
		{
			
		}
		
		protected bool DoRotationCalculations = true;

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

		private double _useItemProgress = 0d;
		private double UseItemIntervalProgress
		{
			get
			{
				return _useItemProgress;
			}
			set
			{
				_useItemProgress = Math.Clamp(value, 0d, 1d);
				AnimationController.Runtime?.Environment?.SetValue("variable.use_item_interval_progress", new DoubleValue(_useItemProgress));
			}
		}
		
		private double _useItemStartupProgress = 0d;
		private double UseItemStartupProgress
		{
			get
			{
				return _useItemStartupProgress;
			}
			set
			{
				_useItemStartupProgress = Math.Clamp(value, 0d, 1d);
				AnimationController.Runtime?.Environment?.SetValue("variable.use_item_startup_progress", new DoubleValue(_useItemStartupProgress));
			}
		}

		public virtual void SwingArm(bool broadcast, bool leftHanded)
		{
			_doUseItemAnimation = true;
			//UseItemStartupProgress = 1d;
			//UseItemIntervalProgress = 1d;
			//AnimationController.Runtime?.Environment.SetValue("variable.use_item_interval_progress", new DoubleValue(1d));
			//	bone.Animations.Enqueue(new SwingAnimation(bone, TimeSpan.FromMilliseconds(250)));
		}

		private TimeSpan _deltaTime = TimeSpan.Zero;
		private Vector3 _previousKnownPosition = Vector3.Zero;
		private Vector3 _delta = Vector3.Zero;
		private Stopwatch _deltaStopwatch = Stopwatch.StartNew();
		public long Age { get; set; } = 0;
		public virtual void OnTick()
		{
			Age++;

			if (IsEating && Age >= _eatingEndTime)
			{
				IsEating = false;
			}

			if (IsGrazing && Age >= _grazingEndTime)
			{
				IsGrazing = false;
			}
			
			_deltaTime = _deltaStopwatch.Elapsed;
			_deltaStopwatch.Restart();
			
			var pos = RenderLocation.ToVector3();
			_delta = pos - _previousKnownPosition;
			_previousKnownPosition = pos;
			
			//Age++;
			if (TargetEntityId != -1)
			{
				if (Level.TryGetEntity(TargetEntityId, out var entity))
				{
					var rotationVector = GetTargetRotationVector(entity);

					TargetRotation = rotationVector;
				}
			}
			
			foreach (var entityComponent in EntityComponents)
			{
				if (entityComponent is ITicked ticked)
					ticked.OnTick();
			}

			//Movement?.OnTick();

			HealthManager.OnTick();
			
			//AnimationController?.OnTick();

			if (DoRotationCalculations)
			{
				UpdateRotations();
			}
			else
			{
				KnownPosition.Yaw = KnownPosition.HeadYaw;
			}

			foreach (var effect in _effects.Values.ToArray())
			{
				effect.OnTick(this);
				
				if (effect.HasExpired())
					RemoveEffect(effect.EffectId);
			}
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
			var known = KnownPosition.ToVector3();
			double deltaX = known.X - _previousPosition.X;
			double deltaZ = known.Z - _previousPosition.Z;
			double distSQ = deltaX * deltaX + deltaZ * deltaZ;

			//IsMoving = distSQ > 0f || Velocity.LengthSquared() > 0f;

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
					//KnownPosition.HeadYaw = newRotationYawHead;
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
							Math.Max(1f - ((float)(_turnTicks - turnTicksLimit) / (float)turnTicksLimit), 0f) *
							maximumHeadBodyAngleDifference;
					}
				}

				KnownPosition.Yaw = MathUtils.ConstrainAngle(KnownPosition.Yaw, KnownPosition.HeadYaw,
					maximumHeadBodyAngleDifference);
			}
			Movement?.UpdateTarget();
			
			_previousPosition = known;
		}

		protected virtual void UpdateModelParts()
		{
			if (ModelRenderer == null)
				return;

			ScaleChanged();
			
			//ModelRenderer.GetBone("body", out _body);

			ModelRenderer.GetBone("leftArm", out _leftArmModel);
			ModelRenderer.GetBone("leftItem", out _leftItemModel);
			ModelRenderer.GetBone("rightArm", out _rightArmModel);
			ModelRenderer.GetBone("rightItem", out _rightItemModel);

		//	ModelRenderer.GetBone("rightLeg", out _rightLegModel);
		//	ModelRenderer.GetBone("leftLeg", out _leftLegModel);

			ModelRenderer.GetBone("head", out _head);
		}

		protected virtual void OnModelUpdated()
		{
			
		}

		private BoundingBox _boundingBox = new BoundingBox();
		public BoundingBox BoundingBox => _boundingBox;

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

			var renderer = ModelRenderer;
			if (renderer != null)
			{
				width = renderer.VisibleBoundsWidth;
				height = renderer.VisibleBoundsHeight;
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

		protected void ToggleCubes(ModelBone bone, bool isInvisible)
		{
			bone.Rendered = !isInvisible;
		}

		private bool _disposed = false;
		public void Dispose()
		{
			if (_disposed)
				return;

			try
			{
				Texture = null;
				
				var model = ModelRenderer;
				ModelRenderer = null;
				model?.Dispose();
				
				OnDispose();
			}
			finally
			{
				_disposed = true;
			}
		}

		protected virtual void OnDispose()
		{
			
		}
		
		/// <inheritdoc />
		public override string ToString()
		{
			return base.ToString();
		}

		private ConcurrentDictionary<EffectType, Effect> _effects = new ConcurrentDictionary<EffectType, Effect>();
		private IItemRenderer                            _itemRenderer    = null;
		private bool _noAi = false;
		private string _nameTag;
		private bool _isUsingItem;
		private bool _isAttacking = false;

		public const float   JumpVelocity = 0.42f;
		public virtual void Jump()
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
			//	Network?.EntityAction((int) EntityId, EntityAction.Jump);
		}
		
		public void AddOrUpdateEffect(Effect effect)
		{
			var effect1 = effect;
			
			effect = _effects.AddOrUpdate(effect.EffectId, effect, (type, e) => effect1);
			effect?.Add(this);
		}

		public void RemoveEffect(EffectType effectType)
		{
			if (_effects.TryRemove(effectType, out var removed))
			{
				removed.Remove(this);
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
		
		public void LookAt(Vector3 targetPosition, bool aimWithHead)
		{
			var    view        = targetPosition - KnownPosition.ToVector3();
			var    dz          = view.Z;
			var    dx          = view.X;
		    
			float tanOutput   = 90f - MathUtils.RadianToDegree(MathF.Atan(dx / (dz)));
			float thetaOffset = 270f;
			if (dz < 0)
			{
				thetaOffset = 90;
			}
			var yaw = thetaOffset + tanOutput;

			if (aimWithHead)
			{
				var bDiff = MathF.Sqrt((dx * dx) + (dz * dz));
				var dy    = (KnownPosition.Y + (float)Height) - (targetPosition.Y);
				KnownPosition.Pitch = MathUtils.RadianToDegree(MathF.Atan(dy / (bDiff)));
			}

			KnownPosition.Yaw = yaw;
			KnownPosition.HeadYaw = yaw;
		}

		public Vector2 GetTargetRotationVector(Entity target)
		{
			var targetPosition = target.KnownPosition.ToVector3();
			var view = targetPosition - KnownPosition.ToVector3();
			var dz = view.Z;
			var dx = view.X;

			float tanOutput = 90f - MathUtils.RadianToDegree(MathF.Atan(dx / (dz)));
			float thetaOffset = 270f;

			if (dz < 0)
			{
				thetaOffset = 90;
			}

			var yaw = thetaOffset + tanOutput;


			var bDiff = MathF.Sqrt((dx * dx) + (dz * dz));
			var dy = (float)(KnownPosition.Y + Height) - (targetPosition.Y);
			var pitch = MathUtils.RadianToDegree(MathF.Atan(dy / (bDiff)));

			return new Vector2(pitch, yaw);
		}

		public void UpdateAttributes(EntityAttributes attributes)
		{
			foreach (var attribute in attributes)
			{
				switch (attribute.Key)
				{
					case "minecraft:movement":
						MovementSpeed = attribute.Value.Value;
						break;
					case "minecraft:health":
						HealthManager.Health = attribute.Value.Value;
						HealthManager.MaxHealth = attribute.Value.MaxValue;
						break;
					default:
						if (!TryUpdateAttribute(attribute.Value))
							Log.Debug($"Unknown attribute: {attribute.Key} (Value={attribute.Value.Value} Min={attribute.Value.MinValue} Max={attribute.Value.MaxValue})");
						break;
				}
			}
		}

		protected virtual bool TryUpdateAttribute(EntityAttribute attribute)
		{
			return false;
		}

		#region MoLang Functions

		[MoProperty("distance_from_camera")]
		public double DistanceFromCamera => Vector3.Distance(Level.Camera.Position, KnownPosition);
		
		/// <summary>
		///		takes one optional hand slot as a parameter (0 or 'main_hand' for main hand, 1 or 'off_hand' for off hand),
		///		and a second parameter (0=default) if you would like the equipped item or any non-zero number for the currently rendered item
		/// </summary>
		/// <param name="mo"></param>
		/// <returns>returns the name of the item in the requested slot (defaulting to the main hand if no parameter is supplied) if there is one, otherwise returns ''.</returns>
		[MoFunction("get_equipped_item_name")]
		public string GetEquippedItemName(MoParams mo)
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

			if (!isOffHand) item = Inventory.MainHand;
			else item = Inventory.OffHand;

			if (item?.Name == null) return "";

			return item.Name.Replace("minecraft:", "");
		}
		
		/// <summary>
		///		Takes one argument that represents the desired axis (0 == x-axis, 1 == y-axis, 2 == z-axis).
		/// </summary>
		/// <param name="axis"></param>
		/// <returns>Returns the absolute position of an actor.</returns>
		[MoFunction("position")]
		public double GetPosition(int axis)
		{
			double amount = 0d;

			switch (axis)
			{
				case 0: //X-Axis
					amount = KnownPosition.X;

					break;

				case 1: //Y-Axis
					amount = KnownPosition.Y;

					break;

				case 2: //Z-Axis
					amount = KnownPosition.Z;

					break;
			}

			return amount;
		}
		
		[MoFunction("life_time")]
		public double GetLifeTime()
		{
			return LifeTime.TotalSeconds;
		}
		
		/// <summary>
		///		Takes one argument that represents the desired axis (0 == x-axis, 1 == y-axis, 2 == z-axis).
		/// </summary>
		/// <param name="axis"></param>
		/// <returns>Returns the position delta for an actor.</returns>
		[MoFunction("position_delta")]
		public double PositionDelta(int axis)
		{
			double amount = 0d;
			switch (axis)
			{
				case 0: //X-Axis
					amount = _delta.X;
					break;
				case 1: //Y-Axis
					amount = _delta.Y;
					break;
				case 2: //Z-Axis
					amount = _delta.Z;
					break;
			}
				
			return amount;
		}

		/// <summary>
		///		Returns the rotation of the camera
		/// </summary>
		/// <param name="axis"></param>
		/// <returns>Requires one argument representing the rotation axis you would like (`0==x`, `1==y`)</returns>
		[MoFunction("camera_rotation")]
		public double CameraRotation(int axis = 0)
		{
			if (axis == 1)
			{
				return Level.Camera.Rotation.Y;
				//Y Axis
			}
			else
			{
				//X Axis
				return Level.Camera.Rotation.X;
			}
		}

		[MoFunction("modified_distance_moved")]
		public double ModifiedDistanceMoved()
		{
			return Movement.DistanceMoved;
		}
		
		[MoFunction("modified_move_speed")]
		public double ModifiedMoveSpeed()
		{
			return (1f / (CalculateMovementSpeed() * 43f)) * (Movement.MetersPerSecond);
		}

		[MoFunction("time_stamp")]
		public double TimeStamp()
		{
			return (double) Level.Time;
		}

		[MoFunction("frame_alpha")]
		public double FrameAlpha()
		{
			return (1f / 50f) * _deltaTime.TotalMilliseconds;
		}

		[MoFunction("delta_time")]
		public double DeltaTime()
		{
			return _deltaTime.TotalSeconds;
		}

		[MoFunction("ground_speed")]
		public double GroundSpeed()
		{
			return Movement.MetersPerSecond;
		}
		
		[MoFunction("walk_distance")]
		public double WalkDistance()
		{
			return Movement.DistanceMoved;
		}
		
		[MoFunction("vertical_speed")]
		public double VerticalSpeed()
		{
			return Movement.VerticalSpeed;
		}
		
		[MoFunction("time_of_day")]
		public double TimeOfDay()
		{
			return ((1f / 24000f) * Level.TimeOfDay);
		}

		/// <summary>
		///		takes one optional hand slot as a parameter (0 or 'main_hand' for main hand, 1 or 'off_hand' for off hand)
		/// </summary>
		/// <param name="mo"></param>
		/// <returns>returns 1.0 if there is an item in the requested slot (defaulting to the main hand if no parameter is supplied), otherwise returns 0.0.</returns>
		[MoFunction("is_item_equipped")]
		public bool IsItemEquipped(MoParams mo)
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
			Item item = isMainHand ? Inventory.MainHand : Inventory.OffHand;

			return item.Count > 0 && !(item is ItemAir);
		}

		[MoProperty("has_target")]
		public bool HasTarget => TargetEntityId != -1;
		
		[MoProperty("has_owner")]
		public bool HasOwner => OwnerEntityId != -1;

		[MoProperty("target_x_rotation")]
		public double TargetXRotation => TargetRotation.X;
		
		[MoProperty("target_y_rotation")]
		public double TargetYRotation => TargetRotation.Y;

		/// <summary>
		///		Returns true if the player has selected an item in the inventory
		/// </summary>
		[MoProperty("is_selected_item")]
		public bool IsSelectedItem => Inventory.MainHand.Count > 0 && !(Inventory.MainHand is ItemAir);

		[MoProperty("main_hand_item_use_duration")]
		public double MainHandItemUseDuration => ItemInUseDuration;

		[MoProperty("main_hand_item_max_duration")]
		public double MainHandItemMaxDuration { get; set; } = 1d;

		[MoProperty("cape_flap_amount")]
		public double CapeFlapAmount { get; set; } = 0d;

		/// <summary>
		///		Gets specified axis of the specified locator offset of the root model
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		[MoFunction("get_root_locator_offset")]
		public double GetRootLocatorOffset(MoParams param)
		{
			//Log.Info($"Root locator params: {param.ToString()}");
			return 0d;
		}

		/// <summary>
		///		Debug log a value to the output debug window for builds that have one
		/// </summary>
		/// <param name="param"></param>
		[MoFunction("debug_output", "debug")]
		public void DebugOutput(MoParams param)
		{
			var values = param.GetParams();

			if (values != null && values.Length > 0)
			{
				var str = string.Join(' ', values.Select(x => x.AsString()));
				Log.Debug(str);
			}
		}

		/// <summary>
		///		takes one optional hand slot as a parameter (0 or 'main_hand' for main hand, 1 or 'off_hand' for off hand)
		/// </summary>
		/// <param name="param"></param>
		/// <returns>returns 1.0 if the item is charged in the requested slot (defaulting to the main hand if no parameter is supplied), otherwise returns 0.0.</returns>
		[MoFunction("item_is_charged")]
		public double ItemIsCharged(MoParams param)
		{
			return 0d;
		}

		[MoProperty("item_max_use_duration")] 
		public double ItemMaxUseDuration => 0d;//MainHandItemMaxDuration;
		
		[MoProperty("item_remaining_use_duration")]
		public double ItemRemainingUseDuration => ItemMaxUseDuration > 0d ? (ItemMaxUseDuration - ItemInUseDuration) : 0d;

		#endregion
	}
}