using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.Blocks.Minecraft;
using Alex.Common.Entities.Properties;
using Alex.Common.Graphics;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Resources;
using Alex.Common.Utils;
using Alex.Common.World;
using Alex.Entities.Components;
using Alex.Entities.Components.Effects;
using Alex.Entities.Properties;
using Alex.Gamestates;
using Alex.Graphics.Camera;
using Alex.Graphics.Models;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.Graphics.Models.Items;
using Alex.Gui;
using Alex.Gui.Elements.Map;
using Alex.Items;
using Alex.MoLang.Attributes;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Alex.Utils.Inventories;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Java;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Utils.Metadata;
using NLog;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using HealthManager = Alex.Entities.Meta.HealthManager;
using Inventory = Alex.Utils.Inventories.Inventory;
using MathF = System.MathF;
using MetadataByte = Alex.Networking.Java.Packets.Play.MetadataByte;
using ModelBone = Alex.Graphics.Models.ModelBone;
using PlayerLocation = Alex.Common.Utils.Vectors.PlayerLocation;

namespace Alex.Entities
{
	public class Entity : MoLangEnvironment
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Entity));
		
		internal EntityDescription Description { get; set; } = null;
		
		public ResourceLocation Type { get; set; } = null;
		private ModelRenderer _modelRenderer;

		public MapIcon MapIcon { get; protected set; }

		public ModelRenderer ModelRenderer
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
						if (_texture != null)
						{
							value.Texture = _texture;
						}

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
		
		public MovementComponent Movement { get; }
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

		internal virtual PlayerLocation RenderLocation { get; set; }

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
			get => _noAi || _scale <= 0.01f;
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

		[MoProperty("model_scale")] public float ModelScale { get; set; } = 1f;
		
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
		
		//public Color? EntityColor { get; set; } = null;
		
		public Inventory Inventory { get; protected set; }

		public IItemRenderer ItemRenderer
		{
			get => _itemRenderer;
			private set
			{
				var oldValue = _itemRenderer;

				UpdateItemPosition(oldValue, value);
				_itemRenderer = value;
			}
		}

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
					var newValue = _itemRenderer;
					UpdateItemPosition(_itemRenderer, newValue);
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
		
		public AnimationComponent AnimationController { get; }
		public TimeSpan LifeTime => DateTime.UtcNow - _timeOfCreation;
		private readonly DateTime _timeOfCreation;
		private Stopwatch _lifeTime;

		public float Experience { get; set; } = 0;
		public float ExperienceLevel { get; set; } = 0; 
		protected Stack<IEntityComponent> EntityComponents { get; }
		public EffectManagerComponent Effects { get; }
		public Entity(World level)
		{
			Structs.Add("query", new ObjectStruct(this));
			
			_timeOfCreation = DateTime.UtcNow;
			EntityComponents = new Stack<IEntityComponent>();
			
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
			EntityComponents.Push(HealthManager);
			
			UUID = new MiNET.Utils.UUID(Guid.NewGuid().ToByteArray());
			
			//BaseMovementSpeed = 4.317D;
			
			AddOrUpdateProperty(new FlyingSpeedProperty(this));
			AddOrUpdateProperty(new MovementSpeedProperty(this));
			AddOrUpdateProperty(new AttackSpeedProperty(this));
			
			Movement = new MovementComponent(this);
			EntityComponents.Push(Movement);
			AnimationController = new AnimationComponent(this);
			//EntityComponents.Push(Movement = new EntityMovement(this));
			EntityComponents.Push(AnimationController);
			EntityComponents.Push(Effects = new EffectManagerComponent(this));
			
			//Effect = new EntityEffect();
			//Effect.Texture = _texture;
			//Effect.VertexColorEnabled = true;

			MapIcon = new EntityMapIcon(this, MapMarker.SmallBlip);
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

		public double AttackSpeed
		{
			get
			{
				return _entityProperties[Networking.Java.Packets.Play.EntityProperties.AttackSpeed].Value;
			}
			set
			{
				_entityProperties[Networking.Java.Packets.Play.EntityProperties.AttackSpeed].Value = value;
			}
		}

		protected void InvokeControllerUpdate()
		{
			AnimationController?.InvokeRenderControllerUpdate();
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

		public double CalculateMovementFactor()
		{
			var modifier =
				(_entityProperties[
					IsFlying ? Networking.Java.Packets.Play.EntityProperties.FlyingSpeed :
						Networking.Java.Packets.Play.EntityProperties.MovementSpeed]).Calculate();

			//if (IsSneaking)
			//	modifier *= 0.29997683577f;
			
			return (modifier);
		}

		private bool _skipRendering = false;
		private void ScaleChanged()
		{
			var modelRenderer = ModelRenderer;

			if (modelRenderer != null)
			{
				var root = modelRenderer?.Model?.Root;

				if (root != null)
					root.BaseScale = Vector3.One * _scale;
					
				if (_scale <= 0.01f)
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

			if ((inHand == null || inHand.IsAir()) && ItemRenderer != null
			                                                            && ModelRenderer != null)
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
						return;
					}

					if (renderer != ItemRenderer)
					{
						var newRenderer = renderer.CloneItemRenderer();
						ItemRenderer = newRenderer;
					}
				}
			}
			else
			{
				if (ItemRenderer != null)
				{
					ItemRenderer = null;
				}
			}
		}

		protected virtual ModelBone GetPrimaryArm()
        {
	        var modelRenderer = ModelRenderer;

	        if (modelRenderer == null)
		        return null;
	        
	        ModelBone arm = null;

	        if (modelRenderer.GetBone("rightItem", out arm) 
	            || modelRenderer.GetBone("leftItem", out arm))
	        {
		        return arm;
	        }

	        return null;
        }

        protected virtual void UpdateItemPosition(IItemRenderer oldValue, IItemRenderer renderer)
        {
	        //ModelBone arm = GetPrimaryArm();
	        
	        if (oldValue != renderer)
	        {
		        if (oldValue != null)
		        {
			        //oldValue.Dispose();
			        //arm?.Remove(oldValue);
		        }
	        }

	        if (renderer?.Model == null)
		        return;

	        renderer.DisplayPosition = DisplayPosition.ThirdPersonRightHand;

	       // if (oldValue != renderer)
	        {
				//	arm?.AddChild(renderer);
	        }
        }

        private void InventoryOnSelectedHotbarSlotChanged(object? sender, SelectedSlotChangedEventArgs e)
        {
	        CheckHeldItem();
        }

		protected virtual void OnInventorySlotChanged(object sender, SlotChangedEventArgs e)
		{
			CheckHeldItem();
		}

		public bool CanPowerJump { get; set; } = false;
		public bool CanClimb { get; set; } = false;
		public bool CanSwim { get; set; } = false;
		
		public double AttackTime => -1d;
		
		[MoProperty("is_onfire"), MoProperty("is_on_fire")]
		public bool IsOnFire { get; set; } = false;

		[MoProperty("is_sneaking")]
		public bool IsSneaking
		{
			get => _isSneaking;
			set
			{
				if (_isSneaking != value)
				{
					var oldValue = _isSneaking;
					_isSneaking = value;

					OnSneakingChanged(value);
				}
			}
		}

		[MoProperty("is_riding")]
		public bool IsRiding { get; set; }

		private static readonly MiNET.Utils.UUID SprintingModifierGuid = new MiNET.Utils.UUID("662A6B8D-DA3E-4C1C-8813-96EA6097278D");
		[MoProperty("is_sprinting")]
		public virtual bool IsSprinting
		{
			get => _isSprinting;
			set
			{
				if (_isSprinting != value)
				{
					_isSprinting = value;
					OnSprintingChanged(value);
				}
			}
		}

		private DateTime _startOfItemUse = DateTime.UtcNow;
		private int _usingSlot = 0;
		[MoProperty("is_using_item")]
		public bool IsUsingItem
		{
			get => _isUsingItem;
			set
			{
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
		public bool IsCharging { get; set; }
		
		[MoProperty("is_angry")]
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

		/// <summary>
		///		Returns the time in seconds since the current animation started, else 0.0 if not called within an animation
		/// </summary>
		[MoProperty("anim_time")]
		public double AnimationTime { get; set; } = 0d;
		
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
		//public bool ShowItemInHand { get; set; } = false;
		public long TargetEntityId { get; set; } = -1;
		public long OwnerEntityId { get; set; } = -1;
		public Vector2 TargetRotation { get; private set; } = Vector2.Zero;

		protected virtual void OnSneakingChanged(bool newValue)
		{
			
		}

		protected virtual void OnSprintingChanged(bool newValue)
		{
			
		}
		
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

					case (int) MiNET.Entities.Entity.MetadataFlags.Color:
					{
						if (meta.Value is MiNET.Utils.Metadata.MetadataInt color)
						{
							if (_modelRenderer != null)
							{
								_modelRenderer.EntityColor = new Microsoft.Xna.Framework.Color((uint)color.Value).ToVector3();
							}

						//	EntityColor = new Microsoft.Xna.Framework.Color((uint) color.Value);
						}
					} break;

					default:
						if (!HandleMetadata((MiNET.Entities.Entity.MetadataFlags) meta.Key, meta.Value))
						{
							if (LoggingConstants.LogUnimplementedEntityFlags)
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
		public bool IsAttacking
		{
			get => _isAttacking;
			set
			{
				var oldValue = _isAttacking;
				
				if (!oldValue && value)
				{ //_attackStart = DateTime.UtcNow;
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
			BitArray data = new BitArray(
				BitConverter.GetBytes(rawData).Concat(BitConverter.GetBytes(extendedData)).ToArray());
			
			IsInvisible = data[(int) MiNET.Entities.Entity.DataFlags.Invisible];// (data & ((int) MiNET.Entities.Entity.DataFlags.Invisible)) != 0;
			IsSneaking = data[(int) MiNET.Entities.Entity.DataFlags.Sneaking];
			IsSitting = data[(int) MiNET.Entities.Entity.DataFlags.Sitting];
			
			IsOnFire = data[(int) MiNET.Entities.Entity.DataFlags.OnFire];
			
			IsSprinting = data[(int) MiNET.Entities.Entity.DataFlags.Sprinting];
			
			NoAi = data[(int) MiNET.Entities.Entity.DataFlags.NoAi];
			IsAffectedByGravity = data[(int) MiNET.Entities.Entity.DataFlags.AffectedByGravity];
			//HasCollision = data[(int) MiNET.Entities.Entity.DataFlags.HasCollision];
			
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

			CanSwim = data[(int) MiNET.Entities.Entity.DataFlags.CanSwim];
			CanClimb = data[(int) MiNET.Entities.Entity.DataFlags.CanClimb];
			//CanFly = data[(int) MiNET.Entities.Entity.DataFlags.CanFly];
			CanPowerJump = data[(int) MiNET.Entities.Entity.DataFlags.CanPowerJump];
			//IsFlying = data[(int) MiNET.Entities.Entity.DataFlags.fl]
		}

		private Texture2D _texture;

		public Texture2D Texture
		{
			get
			{
				return _texture;
			}
			set
			{
				//if (value == _texture)
				//	return;
				
				Texture2D oldValue = _texture;

				try
				{
					_texture = value;

					if (_modelRenderer != null)
					{
						_modelRenderer.Texture = value;
					}
				}
				finally
				{
					OnTextureChanged(oldValue, value);
					//oldValue?.Dispose();
					//oldValue?.Release(this);
					//oldValue?.ReturnResource(this);
				}
			}
		}

		protected virtual void OnTextureChanged(Texture2D oldValue, Texture2D newValue)
		{
			if (oldValue != null)
			{
				if (!(oldValue.Tag is Guid tag) || tag != EntityFactory.PooledTagIdentifier)
				{
					oldValue?.Dispose();
				}
			}
		}
		
		//internal EntityEffect    Effect       { get; }

		///  <summary>
		/// 		Renders the entity
		///  </summary>
		///  <param name="renderArgs"></param>
		///  <param name="useCulling">True if you want the model to use backface culling.</param>
		///  <returns>The amount of draw calls made</returns>
		public virtual int Render(IRenderArgs renderArgs, bool useCulling)
		{
			if (IsInvisible || _skipRendering)
				return 0;
		//	if (_disposed)
			//	return 0;
			
			
			int renderCount = 0;
			var  renderer = ModelRenderer;

			var worldMatrix = Matrix.CreateScale((1f / 16f) * (ModelScale))
			                  * RenderLocation.CalculateWorldMatrix();

			if (renderer != null)
			{
				renderCount += renderer.Render(renderArgs, worldMatrix);

				//Hack to render held items
				renderCount += RenderHeldItem(renderArgs, renderer, worldMatrix);
			}

			return renderCount;
		}

		protected int RenderHeldItem(IRenderArgs renderArgs, ModelRenderer renderer, Matrix worldMatrix)
		{
			int count = 0;
			var itemRenderer = _itemRenderer;
			if (itemRenderer != null)
			{
				Matrix matrix = worldMatrix;

				if (!IsFirstPersonMode)
				{
					var primaryArm = GetPrimaryArm();

					if (primaryArm != null)
					{
						var bones = renderer.Model.Bones.ImmutableArray;
						Matrix[] matrices = new Matrix[bones.Length];
						Graphics.Models.Model.CopyAbsoluteBoneTransformsTo(bones, matrices);

						if (primaryArm.Index >= 0 && primaryArm.Index < matrices.Length)
						{
							matrix = matrices[primaryArm.Index] * worldMatrix;
						}
					}
				}

				count += itemRenderer?.Render(renderArgs, matrix) ?? 0;
			}

			return count;
		}
		
		private bool _doUseItemAnimation = false;
		public DateTime LastUpdate { get; set; }
		public virtual void Update(IUpdateArgs args)
		{
			var elapsed = Alex.DeltaTime;

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
			}
			
			foreach (var entityComponent in EntityComponents)
			{
				if (!entityComponent.Enabled)
					continue;
				
				if (entityComponent is IUpdated updateable)
				{
					updateable.Update(args.GameTime);
				}
			}

			var renderer = ModelRenderer;

			if (renderer == null || _skipRendering || IsInvisible)
				return;
			//if (((!RenderEntity || IsInvisible) && !ShowItemInHand) || renderer == null || _skipRendering) return;

			var head = _headBone;
			if (head != null)
			{
				var headYaw = (KnownPosition.HeadYaw - KnownPosition.Yaw);
				var pitch = KnownPosition.Pitch;

				head.Rotation = new Vector3(-pitch, headYaw, 0f);
				//_head.Rotation = Quaternion.CreateFromYawPitchRoll(MathUtils.ToRadians(headYaw), MathUtils.ToRadians(pitch), 0f);
			}
			
			renderer.Update(args);
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

		private static readonly MoPath _useItemIntervalProgressMoPath =
			new MoPath("variable.use_item_interval_progress");
		
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
				AnimationController.Runtime?.Environment?.SetValue(_useItemIntervalProgressMoPath, new DoubleValue(_useItemProgress));
			}
		}
		
		private static readonly MoPath _useItemStartupProgressMoPath =
			new MoPath("variable.use_item_startup_progress");
		
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
				AnimationController.Runtime?.Environment?.SetValue(_useItemStartupProgressMoPath, new DoubleValue(_useItemStartupProgress));
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
				if (!entityComponent.Enabled)
					continue;
				
				if (entityComponent is ITicked ticked)
					ticked.OnTick();
			}
			
			if (DoRotationCalculations)
			{
				UpdateRotations();
			}
			else
			{
				KnownPosition.Yaw = KnownPosition.HeadYaw;
			}
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

		private ModelBone _headBone = null;
		protected virtual void UpdateModelParts()
		{
			var modelRenderer = ModelRenderer;

			if (modelRenderer == null)
			{
				_headBone = null;
				return;
			}

			ScaleChanged();

			if (modelRenderer.GetBone("head", out var headBone))
			{
				_headBone = headBone;
			}
			else
			{
				_headBone = null;
			}
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
			var    length     = Width;
			var    width     = Width;
			var    height    = Height;

			if (IsSwimming)
			{
				height = Width;
				width = Width;
				length = Height;
			}
			
			double halfWidth = (width * Scale) / 2D;
			double halfLength = (length * Scale) / 2D;

			return new BoundingBox(
				new Vector3((float) (pos.X - halfWidth), pos.Y, (float) (pos.Z - halfLength)),
				new Vector3(
					(float) (pos.X + halfWidth), (float) (pos.Y + (height * Scale)), (float) (pos.Z + halfLength)));
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

		public virtual float CollidedWithWorld(Vector3 direction, Vector3 position, float impactVelocity)
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

			return 0;
		}

		private bool _disposed = false;
		public void Dispose()
		{
			if (_disposed)
				return;

			try
			{
				ItemRenderer?.Dispose();
				ItemRenderer = null;
				
				ModelRenderer?.Dispose();
				ModelRenderer = null;

				var texture = Texture;

				if (texture != null)
				{
					Texture = null;
					texture?.Dispose();
				}

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
			return $"{{Type: \"{GetType().Name}\", Def: \"{AnimationController?.EntityDefinition?.Identifier}\", EntityId: {EntityId}}}";
		}
		
		private IItemRenderer                            _itemRenderer    = null;
		private bool _noAi = false;
		private string _nameTag;
		private bool _isUsingItem;
		private bool _isAttacking = false;
		private bool _isSprinting;
		private bool _isSneaking;
		private bool _hasRider = false;

		public bool CanSurface { get; set; } = false;
		public const float   JumpVelocity = 0.42f;
		public virtual void Jump()
		{
			HealthManager.IncreaseExhaustion(IsSprinting ? 0.2f : 0.05f);
			var jumpVelocity = JumpVelocity;
			
			if (IsInWater || IsInLava)
			{
				if (((FeetInWater && !HeadInWater) || (FeetInLava && !HeadInLava)) && CanSurface)
				{
					jumpVelocity = 0.4f;
				}
				else
				{
					jumpVelocity = 0.04f;
				}
			}
			else
			{
				jumpVelocity = Effects.ApplyEffect(EffectType.JumpBoost, jumpVelocity);
			}

			Movement.Push(new Vector3(0f, jumpVelocity, 0f));
		}
		
		public virtual void OnSpawn()
		{
			IsSpawned = true;
			
			//HandleEntityFlags(_data, _extendedData);
		}

		public virtual void OnDespawn()
		{
			IsSpawned = false;
		}

		public Vector3 CalculateForwardVector()
		{
			return new Vector3(
				MathF.Cos(KnownPosition.Pitch) * MathF.Sin(KnownPosition.Yaw), 
				-MathF.Sin(KnownPosition.Pitch),
				MathF.Cos(KnownPosition.Pitch) * MathF.Cos(KnownPosition.Yaw));
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
			var dy = (float)(KnownPosition.Y ) - (targetPosition.Y);
			var pitch = MathUtils.RadianToDegree(MathF.Atan(dy / (bDiff)));

			return new Vector2(pitch, yaw);
		}

		public void UpdateAttributes(EntityAttributes attributes)
		{
			foreach (var attribute in attributes)
			{
				switch (attribute.Key)
				{
					case "minecraft:player.level":
						ExperienceLevel = attribute.Value.Value;
						break;
					case "minecraft:player.experience":
						Experience = attribute.Value.Value;
						break;
					case "minecraft:player.saturation":
						HealthManager.Saturation = (int) attribute.Value.Value;
						HealthManager.MaxSaturation = (int) attribute.Value.MaxValue;
						break;
					case "minecraft:player.exhaustion":
						HealthManager.Exhaustion = (int) attribute.Value.Value;
						HealthManager.MaxExhaustion = (int) attribute.Value.MaxValue;
						break;
					case "minecraft:player.hunger":
						HealthManager.Hunger = (int)attribute.Value.Value;
						HealthManager.MaxHunger = (int)attribute.Value.MaxValue;
						break;
					case "minecraft:movement":
						MovementSpeed = attribute.Value.Value;
						break;
					case "health":
					case "minecraft:health":
						if (this is Player)
						{
							Log.Info(
								$"Player health update! {attribute.Value.ToString()}");
						}
						HealthManager.Health = attribute.Value.Value;
						HealthManager.MaxHealth = attribute.Value.MaxValue;
						break;
					case "minecraft:absorption":
						HealthManager.Absorption = attribute.Value.Value;
						//HealthManager.
						break;
					default:
						if (!TryUpdateAttribute(attribute.Value))
							Log.Warn($"Unknown attribute! {attribute.Value.ToString()})");
						break;
				}
			}
		}

		protected virtual bool TryUpdateAttribute(EntityAttribute attribute)
		{
			return false;
		}

		#region MoLang Functions

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

			return item != null && item.Count > 0 && !(item is ItemAir);
		}

		/// <summary>
		///		Gets specified axis of the specified bone orientation pivot
		/// </summary>
		/// <param name="mo"></param>
		/// <returns></returns>
		[MoFunction("get_default_bone_pivot")]
		public double GetDefaultBonePivot(MoParams mo)
		{
			var p1 = mo.Get(0);

			ModelBone bone = null;
			if (p1 is StringValue sv)
			{
				if (_modelRenderer.GetBone(sv.Value, out bone))
				{
					int axis = 0;
					if (mo.Contains(1))
					{
						axis = mo.GetInt(1);
					}
					
					var pivot = bone.Pivot.GetValueOrDefault(Vector3.Zero);

					switch (axis)
					{
						case 0:
							return pivot.X;
						case 1:
							return pivot.Y;
						case 2:
							return pivot.Z;
					}
				}
			}

			return 0d;
		}

		/// <summary>
		///		Returns the current walk speed of the entity modified by status flags such as is_baby or on_fire
		/// </summary>
		[MoProperty("modified_move_speed")]
		public double ModifiedMoveSpeed
		{
			get
			{
				var sp = Movement.MetersPerSecond;

				if (MathF.Abs(sp) < 0.01f)
					return 0;
			
				var maxSpeed = 4.317f;
				if (IsSneaking)
				{
					maxSpeed *= 0.3f;
				}
				else if (IsSprinting)
				{
					//	maxSpeed *= 1.3f;
				}

				maxSpeed *= 10f;
				maxSpeed *= (float)CalculateMovementFactor();
			
				return sp / maxSpeed;
			}
		}
		
		/// <summary>
		///		Returns the distance of the root of this actor or particle emitter from the camera
		/// </summary>
		[MoProperty("distance_from_camera")] public double DistanceFromCamera => Vector3.Distance(Level.Camera.Position, KnownPosition);
		
		/// <summary>
		/// Returns the time in seconds since the current animation started, else 0.0 if not called within an animation
		/// </summary>
		[MoProperty("life_time")] public double GetLifeTime => _lifeTime.Elapsed.TotalSeconds;
		
		/// <summary>
		/// Returns the total distance the entity has moved horizontally in meters (since the entity was last loaded, not necessarily since it was originally created) modified along the way by status flags such as is_baby or on_fire
		/// </summary>
		[MoProperty("modified_distance_moved")] public double ModifiedDistanceMoved => Movement.TotalDistanceMoved;
		
		/// <summary>
		/// Returns the current time stamp of the level
		/// </summary>
		[MoProperty("time_stamp")] public double TimeStamp => (double)Level.Time;
		
		/// <summary>
		///		Returns the ratio (from 0 to 1) of how much between AI ticks this frame is being rendered
		/// </summary>
		[MoProperty("frame_alpha")] public double FrameAlpha => (1f / 50f) * _deltaTime.TotalMilliseconds;
		
		/// <summary>
		///		Returns the time in seconds since the previous frame
		/// </summary>
		[MoProperty("delta_time")] public double DeltaTime => _deltaTime.TotalSeconds;
		
		/// <summary>
		///		Returns the ground speed of the entity in metres/second
		/// </summary>
		[MoProperty("ground_speed")] public double GroundSpeed => Movement.MetersPerSecond;
		
		/// <summary>
		///		Returns the walk distance of the entity.
		/// </summary>
		[MoProperty("walk_distance")] public double WalkDistance => Movement.DistanceMoved;
		
		/// <summary>
		///		Returns the speed of the entity up or down in metres/second, where positive is up
		/// </summary>
		[MoProperty("vertical_speed")] public double VerticalSpeed => Movement.VerticalSpeed;
		
		/// <summary>
		///		Returns the time of day (midnight=0.0, sunrise=0.25, noon=0.5, sunset=0.75) of the dimension the entity is in.
		/// </summary>
		[MoProperty("time_of_day")] public double TimeOfDay => ((1f / 24000f) * Level.TimeOfDay);
		
		/// <summary>
		///		Returns 1.0 if the entity has a target, else it returns 0.0
		/// </summary>
		[MoProperty("has_target")] public bool HasTarget => TargetEntityId != -1;
		
		/// <summary>
		///		Returns true if the entity has an owner ID else it returns false
		/// </summary>
		[MoProperty("has_owner")] public bool HasOwner => OwnerEntityId != -1;
		
		/// <summary>
		///		Returns the x rotation required to aim at the entity's current target if it has one, else it returns 0.0
		/// </summary>
		[MoProperty("target_x_rotation")] public double TargetXRotation => TargetRotation.X;
		
		/// <summary>
		/// Returns the y rotation required to aim at the entity's current target if it has one, else it returns 0.0
		/// </summary>
		[MoProperty("target_y_rotation")] public double TargetYRotation => TargetRotation.Y;

		/// <summary>
		///		Returns true if the entity has a rider, else it returns false
		/// </summary>
		[MoProperty(("has_rider"))]
		public bool HasRider
		{
			get => _hasRider;
			set
			{
				_hasRider = value;
				InvokeControllerUpdate();
			}
		}

		/// <summary>
		///		Returns true if the player has selected an item in the inventory
		/// </summary>
		[MoProperty("is_selected_item")] public bool IsSelectedItem => Inventory.MainHand.Count > 0 && !(Inventory.MainHand is ItemAir);
		
		/// <summary>
		///		Returns the use time for the main hand item.
		/// </summary>
		[MoProperty("main_hand_item_use_duration")] public double MainHandItemUseDuration => ItemInUseDuration;

		/// <summary>
		///		Returns the use time maximum duration for the main hand item if it makes sense, else it returns 0.0
		/// </summary>
		[MoProperty("main_hand_item_max_duration")] public double MainHandItemMaxDuration { get; set; } = 1d;

		/// <summary>
		///		returns value between 0.0 and 1.0 with 0.0 meaning cape is fully down and 1.0 is cape is fully up
		/// </summary>
		[MoProperty("cape_flap_amount")] public double CapeFlapAmount { get; set; } = 0d;

		/// <summary>
		///			returns the hurt time for the actor, otherwise returns 0
		/// </summary>
		[MoProperty("hurt_time")] public double HurtTime { get; set; } = 0d;
		
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
		///		Takes one argument as a parameter. Returns the nth head x rotation of the entity if it makes sense, else it returns 0.0
		/// </summary>
		/// <param name="moParams"></param>
		/// <returns></returns>
		[MoFunction("head_x_rotation")]
		public double HeadXRotation(MoParams moParams)
		{
			int headNumber = 0;

			if (moParams.Contains(0))
			{
				headNumber = moParams.GetInt(0);
			}
			
			if (ModelRenderer.GetBone(headNumber > 0 ? $"head{headNumber}" : "head", out var bone))
			{
				return bone.Rotation.X;
			}
			return 0d;
		}
		
		/// <summary>
		///		Takes one argument as a parameter. Returns the nth head y rotation of the entity if it makes sense, else it returns 0.0
		/// </summary>
		/// <param name="moParams"></param>
		/// <returns></returns>
		[MoFunction("head_y_rotation")]
		public double HeadYRotation(MoParams moParams)
		{
			int headNumber = 0;

			if (moParams.Contains(0))
			{
				headNumber = moParams.GetInt(0);
			}
			
			if (ModelRenderer.GetBone(headNumber > 0 ? $"head{headNumber}" : "head", out var bone))
			{
				return bone.Rotation.Y;
			}
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

		/// <summary>
		///		Returns the maximum amount of time the item can be used, else 0.0 if it doesn't make sense
		/// </summary>
		[MoProperty("item_max_use_duration")] 
		public double ItemMaxUseDuration => 0d;//MainHandItemMaxDuration;
		
		/// <summary>
		///		Returns the amount of time an item has left to use, else 0.0 if it doesn't make sense.
		///		Item queried is specified by the slot name 'main_hand' or 'off_hand'.
		///		Time remaining is normalized using the normalization value, only if one is given, else it is returned in seconds.
		/// </summary>
		[MoProperty("item_remaining_use_duration")] public double ItemRemainingUseDuration => ItemMaxUseDuration > 0d ? (ItemMaxUseDuration - ItemInUseDuration) : 0d;

		/// <summary>
		///		Returns the list of entities within the specified distance, taking an optional second argument as a filter for which mob types to accept (eg: 'minecraft:pig').
		/// </summary>
		/// <param name="maxDistance">the max distance from this entity</param>
		/// <param name="type">filter for which mob types to accept (eg: 'minecraft:pig')</param>
		/// <returns>list of entities</returns>
		[MoFunction("get_nearby_entities")]
		public IEnumerable<Entity> GetNearbyEntities(double maxDistance, string type = "")
		{
			var resourceLocation = new ResourceLocation(type);
			
			bool checkType = !string.IsNullOrWhiteSpace(type);
			foreach (var entity in Level.EntityManager.GetEntities(_knownPosition.ToVector3(), maxDistance))
			{
				if (!checkType || (entity.Type != null && entity.Type.Equals(resourceLocation)))
				{
					yield return entity;
				}
			}
		}

		[MoFunction("moon_phase")]
		public int GetMoonPhase()
		{
			return (int)Level.MoonPhase;
		}
		
		[MoFunction("moon_brightness")]
		public double GetMoonBrightness()
		{
			switch (Level.MoonPhase)
			{
				case MoonPhase.FullMoon:
					return 1;

				case MoonPhase.WaningGibbous:
					return 0.75;

				case MoonPhase.FirstQuarter:
					return 0.5;

				case MoonPhase.WaningCrescent:
					return 0.25;

				case MoonPhase.NewMoon:
					return 0;

				case MoonPhase.WaxingCrescent:
					return 0.25;

				case MoonPhase.LastQuarter:
					return 0.5;

				case MoonPhase.WaxingGibbous:
					return 0.75;
			}

			return 0;
		}

		#endregion
	}
}