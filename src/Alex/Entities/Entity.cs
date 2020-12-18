using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API;
using Alex.API.Data.Servers;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Network;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Entities.Effects;
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
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using MathF = System.MathF;
using MetadataByte = Alex.Networking.Java.Packets.Play.MetadataByte;
using MetadataFloat = MiNET.Utils.MetadataFloat;
using MetadataString = MiNET.Utils.MetadataString;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Entities
{
	public class Entity : IEntity, IPhysicsEntity
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
				_modelRenderer = value;
				UpdateModelParts();
				OnModelUpdated();
			}
		}
		
		public EntityMovement Movement { get; }

		public World Level { get; set; }
		
		public long EntityId { get; set; }
		public bool IsSpawned { get; set; }

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

		public  Vector3      Velocity      { get; set; } = Vector3.Zero;

		/// <inheritdoc />
		public PhysicsState PreviousState { get; set; } = new PhysicsState(Vector3.Zero, new PlayerLocation());
		
		private float        _posOffset    = 0;
		private bool         _posOffsetSet = false;

		public float PositionOffset
		{
			get
			{
				if (!_posOffsetSet)
					return (float) Height;
				return _posOffset;
			}
			set
			{
				_posOffsetSet = true;
				_posOffset = value;
			}
		}

		//public HealthManager HealthManager { get; set; }
		public string NameTag { get; set; }

		public virtual bool NoAi { get; set; } = true;
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
		
		public MiNET.Utils.UUID UUID { get; set; } = new MiNET.Utils.UUID(Guid.Empty.ToByteArray());

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
		public IItemRenderer ItemRenderer { get; private set; } = null;
		
		private EntityModelRenderer.ModelBone _leftArmModel;
		private EntityModelRenderer.ModelBone _rightArmModel;
		private EntityModelRenderer.ModelBone _rightItemModel;
		
		private EntityModelRenderer.ModelBone _leftLegModel;
		private EntityModelRenderer.ModelBone _rightLegModel;

		private EntityModelRenderer.ModelBone _body;
		private EntityModelRenderer.ModelBone _head;
		
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
					CheckHeldItem();
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
            Inventory = new Inventory(46);
            //	HealthManager = new HealthManager(this);

            Inventory.SlotChanged += OnInventorySlotChanged;
            Inventory.SelectedHotbarSlotChanged += InventoryOnSelectedHotbarSlotChanged;

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

			return (modifier)- 0.00475f;
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
		}

        private void CheckHeldItem()
        {
            var inHand = Inventory.MainHand;

            if ((inHand == null || inHand.Count == 0 || inHand.Id <= 0) && ItemRenderer != null && ModelRenderer != null)
            {
                if (ModelRenderer.GetBone("rightItem", out EntityModelRenderer.ModelBone bone))
                {
	                bone.Remove(ItemRenderer);
                }

                ItemRenderer = null;
                return;
            }

            if (inHand != null)
            {
	            if (!string.IsNullOrWhiteSpace(inHand.Name))
	            {
		            var renderer = inHand?.Renderer;
		            if (renderer == null)
		            { 
			            //Log.Warn($"No renderer for item: {inHand.Name}");
			            return;
		            }

		            if (renderer == ItemRenderer)
			            return;

		            renderer = renderer.Clone();
		            
		            var itemModel = renderer.Model;

		            var oldRenderer = ItemRenderer;
		            if (oldRenderer != default)
		            {
			            renderer.DisplayPosition = oldRenderer.DisplayPosition;
		            }

		            if (inHand is ItemBlock)
		            {
			            renderer.Scale = new Vector3(0.5f);
		            }
		            
		           // renderer.Scale = new Vector3(_scale);

		            ItemRenderer = renderer;
					
		            if (this is RemotePlayer p)
		            {
			            var pos = renderer.DisplayPosition;
			            //if (pos.HasFlag(DisplayPosition.FirstPerson) || pos.HasFlag(DisplayPosition.ThirdPerson))
			            {
				            if (p.IsLeftHanded)
				            {
					            if (!pos.HasFlag(DisplayPosition.LeftHand))
					            {
						            pos = (pos & ~(DisplayPosition.LeftHand | DisplayPosition.RightHand));
						            pos |= DisplayPosition.LeftHand;
					            }
				            }
				            else
				            {
					            if (!pos.HasFlag(DisplayPosition.RightHand))
					            {
						            pos = (pos & ~(DisplayPosition.LeftHand | DisplayPosition.RightHand));
						            pos |= DisplayPosition.RightHand;
					            }
				            }

				            if (p.IsFirstPersonMode)
				            {
					            if (!pos.HasFlag(DisplayPosition.FirstPerson))
					            {
						            pos = (pos & ~(DisplayPosition.FirstPerson | DisplayPosition.ThirdPerson));
						            pos |= DisplayPosition.FirstPerson;
					            }
				            }
				            else
				            {
					            if (!pos.HasFlag(DisplayPosition.ThirdPerson))
					            {
						            pos = (pos & ~(DisplayPosition.FirstPerson | DisplayPosition.ThirdPerson));
						            pos |= DisplayPosition.ThirdPerson;
					            }
				            }

				            renderer.DisplayPosition = pos;
			            }
		            }
		            else
		            {
			            renderer.DisplayPosition = DisplayPosition.ThirdPersonRightHand;
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
				switch ((MiNET.Entities.Entity.MetadataFlags) meta.Key)
				{
					case MiNET.Entities.Entity.MetadataFlags.CollisionBoxHeight:
					{
						if (meta.Value is MetadataFloat flt)
						{
							Height = flt.Value;
						}
					} break;
					case MiNET.Entities.Entity.MetadataFlags.CollisionBoxWidth:
					{
						if (meta.Value is MetadataFloat fltw)
						{
							Width = fltw.Value;
						//	Length = fltw.Value;
						}
					} break;
					case MiNET.Entities.Entity.MetadataFlags.Scale:
					{
						if (meta.Value is MetadataFloat flt)
						{
							Scale = flt.Value;
						}
					} break;
					case MiNET.Entities.Entity.MetadataFlags.EntityFlags:
					{
						if (meta.Value is MetadataLong lng)
						{
							HandleEntityFlags(lng.Value);
						}
					}
						break;
					case MiNET.Entities.Entity.MetadataFlags.NameTag:
					{
						if (meta.Value is MetadataString nametag)
						{
							NameTag = nametag.Value;
						}
					}
						break;

					case MiNET.Entities.Entity.MetadataFlags.HideNameTag:
					{
						if (meta.Value is MiNET.Utils.MetadataByte hideNameTag)
						{
							HideNameTag = hideNameTag.Value == 1;
						}
						break;
					}
					
					default:
						if (!HandleMetadata((MiNET.Entities.Entity.MetadataFlags) meta.Key, meta.Value))
						{
							Log.Debug($"Unknown flag: {(MiNET.Entities.Entity.MetadataFlags) meta.Key}");
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
		
		/// <inheritdoc />
		public long RenderedVertices { get; private set; }

		public virtual void Render(IRenderArgs renderArgs)
		{
			var  renderer = ModelRenderer;
			long rendered = 0;

			if (((RenderEntity && !IsInvisible) || ShowItemInHand) && renderer != null && !_skipRendering)
			{
				renderer.Render(renderArgs, IsInvisible || !RenderEntity);

				if (renderer.Valid)
				{
					rendered += renderer.Vertices;
				}
			}
			
			if (ShowItemInHand && ItemRenderer != null && !_skipRendering)
			{
				//ItemRenderer.
				ItemRenderer.Render(renderArgs, false, out int itemVertices);
				rendered += itemVertices;

				//rendered += ItemRenderer.VertexCount;
			}

			RenderedVertices = rendered;
		}

		public virtual void Update(IUpdateArgs args)
		{
			var renderer = ModelRenderer;

			if (((!RenderEntity || IsInvisible) && !ShowItemInHand) || renderer == null || _skipRendering) return;
			
			renderer.Update(args, RenderLocation);

            CalculateLegMovement(args);

            if (!ShowItemInHand || _skipRendering || ItemRenderer == null) return;
            
            var pivot = Vector3.Zero;

            if (_rightItemModel != null)
            {
	            pivot = _rightItemModel.Definition.Pivot;
            }
            else if (_rightArmModel != null)
            {
	            pivot = _rightArmModel.Definition.Pivot;
            }
					
            var scaleMatrix = Matrix.Identity;

            if (ItemRenderer != null && _rightArmModel != null)
            {
	            if ((ItemRenderer.DisplayPosition & DisplayPosition.ThirdPerson) != 0)
		            scaleMatrix = Matrix.CreateTranslation(-pivot)
		                          * Matrix.CreateRotationY(
			                          MathUtils.ToRadians((1f / 16f) * _rightArmModel.Rotation.Y))
		                          * Matrix.CreateRotationX(
			                          MathUtils.ToRadians((1f / 16f) * _rightArmModel.Rotation.X))
		                          * Matrix.CreateRotationZ(
			                          MathUtils.ToRadians((1f / 16f) * _rightArmModel.Rotation.Z))
		                          * Matrix.CreateTranslation(pivot);
            }

            ItemRenderer?.Update(
	            args,
	            scaleMatrix * Matrix.CreateScale(Scale)
	                        * Matrix.CreateRotationY(MathHelper.ToRadians(180f - RenderLocation.HeadYaw))
	                        * Matrix.CreateTranslation(RenderLocation.X, RenderLocation.Y, RenderLocation.Z),
	            Color.White.ToVector3(),
	            new PlayerLocation(
		            RenderLocation.X, RenderLocation.Y, RenderLocation.Z, 180f - RenderLocation.HeadYaw,
		            180f - RenderLocation.Yaw, RenderLocation.Pitch));
					
            ItemRenderer?.Update(args.GraphicsDevice, args.Camera);
		}

        public void UpdateHeadYaw(float rotation)
        {
            KnownPosition.HeadYaw = rotation;
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

		private  Vector3 _prevUpdatePosition = Vector3.Zero;
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

			if (bone != null)
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
			float distSQ = Vector3.DistanceSquared(_prevUpdatePosition, pos);

			var distance = DistanceMoved;
			distSQ = distance;

			if (!IsMoving)
			{
				//DistanceMoved = 0f;
			}

			var dt = (float) args.GameTime.ElapsedGameTime.TotalSeconds;

			if (!ServerEntity)
			{
				var distanceMoved = DistanceMoved;
				DistanceMoved = 0;

				_mvSpeed = (float) (distanceMoved * (TimeSpan.FromSeconds(1) / args.GameTime.ElapsedGameTime));
			}

			Animate(distSQ, dt);
			
			_prevUpdatePosition = pos;
		}

		protected virtual void Animate(float distSQ, float dt)
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

				if (_head != null && !_head.IsAnimating)
				{
				//	_head.Position = posOffset;
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
				Vector3 rArmRot = Vector3.Zero;

				if (distSQ > 0f)
				{
					if (!IsMoving)
					{
						_armRotation = 0f;
						IsMoving = true;
					}

					_armRotation += (float) (_mvSpeed) * dt;
					//rArmRot = new Vector3(tcos0, 0, 0);
					rArmRot = new Vector3((0.5f + MathF.Cos(_armRotation)) * 24.5f, 0, 0);
				}
				else
				{
					IsMoving = false;
					_armRotation += dt;

					rArmRot = new Vector3(
						(0.5f + MathF.Cos(_armRotation)) * -7.5f, 0f, 0.1f + (MathF.Sin(_armRotation) * -1.5f));
				}


				if (!_leftArmModel.IsAnimating)
				{
					_leftArmModel.Rotation = rArmRot;
				}

				if (!_rightArmModel.IsAnimating)
				{
					_rightArmModel.Rotation = -rArmRot;
				}
			}


			if (_leftLegModel != null && _rightLegModel != null)
			{
				Vector3 lLegRot = Vector3.Zero;
				Vector3 rLegRot = Vector3.Zero;

				if (distSQ > 0f)
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

		private   DateTime _nextUpdate     = DateTime.MinValue;
		private   DateTime _previousUpdate = DateTime.MinValue;
		protected int      SurroundingLightValue { get; private set; } = 15;
		public virtual void OnTick() 
		{
			SeenEntities.Clear();
			UnseenEntities.Clear();
			
			Age++;

			HealthManager.OnTick();
			
			if (_isHit && Age > _hitAnimationEnd)
			{
				_isHit = false;
				ModelRenderer.EntityColor = Color.White.ToVector3();
			}

			if (DoRotationCalculations)
			{
				UpdateRotations();
			}
			else
			{
				KnownPosition.Yaw = KnownPosition.HeadYaw;
			}

			_previousPosition = KnownPosition;

			if (ServerEntity && DateTime.UtcNow >= _nextUpdate)
			{
				var distanceMoved = DistanceMoved;
				DistanceMoved = 0;

				//PreviousUpdate
				_mvSpeed = (float) (distanceMoved * (TimeSpan.FromSeconds(1) / (DateTime.UtcNow - _previousUpdate)));
				
				_previousUpdate = DateTime.UtcNow;
				_nextUpdate = DateTime.UtcNow + TimeSpan.FromMilliseconds(500);
			}

			if (IsRendered || !ServerEntity)
			{
				SurroundingLightValue = 15; /*Math.Min(
					Level.GetSkyLight(KnownPosition) + Level.GetBlockLight(KnownPosition), 15);

				var heldItemRenderer = ItemRenderer;

				if (heldItemRenderer != null)
				{
					heldItemRenderer.DiffuseColor =  (new Color(245, 245, 225) *  ((1f / 16f) * SurroundingLightValue))
					                                * Level.BrightnessModifier;
				}*/
			}

			foreach (var effect in _effects.Values.ToArray())
			{
				effect.OnTick(this);
			}

			if (IsNoAi) return;
		//	IsMoving = Velocity.LengthSquared() > 0f;

		var knownPos = new BlockCoordinates(new Vector3(KnownPosition.X, KnownPosition.Y, KnownPosition.Z));
		var knownDown = KnownPosition.GetCoordinates3D();

	//	if (Alex.ServerType == ServerType.Bedrock)
		{
			knownDown = knownDown.BlockDown();
		}
		
			var blockBelowFeet = Level?.GetBlockStates(knownDown.X, knownDown.Y, knownDown.Z);
			var feetBlock = Level?.GetBlockStates(knownPos.X, knownPos.Y, knownPos.Z).ToArray();
			var headBlock = Level?.GetBlock(KnownPosition.GetCoordinates3D() + new BlockCoordinates(0, 1, 0));

			if (headBlock != null)
			{
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
			ModelRenderer.GetBone("rightArm", out _rightArmModel);
			ModelRenderer.GetBone("rightItem", out _rightItemModel);

			ModelRenderer.GetBone("rightLeg", out _rightLegModel);
			ModelRenderer.GetBone("leftLeg", out _leftLegModel);

			if (ModelRenderer.GetBone("head", out _head))
			{
				_head.ApplyPitch = true;
				_head.ApplyHeadYaw = true;
			}

			/*if (ModelRenderer.GetBone("hat", out EntityModelRenderer.ModelBone hat))
			{
				foreach (var c in hat.Cubes)
				{
					c.ApplyHeadYaw = true;
					c.ApplyYaw = false;
					c.ApplyPitch = true;
				}
			}*/
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
			double halfWidth = (Width * Scale) / 2D;
			double halfDepth = (Width * Scale) / 2D;
			
			return new BoundingBox(new Vector3((float)(pos.X - halfWidth), pos.Y, (float)(pos.Z - halfDepth)), new Vector3((float)(pos.X + halfWidth), (float)(pos.Y + (Height * Scale)), (float)(pos.Z + halfDepth)));
		}
		
		public bool IsColliding(IEntity other)
		{
			return IsColliding(GetBoundingBox(), other);
		}

		public bool IsColliding(BoundingBox bbox, IEntity other)
		{
			//if (!Compare((int) KnownPosition.X, (int) other.KnownPosition.X, 5)) return false;
			//if (!Compare((int) KnownPosition.Z, (int) other.KnownPosition.Z, 5)) return false;
			if (!Compare((int) KnownPosition.X, (int) other.KnownPosition.X, 4)) return false;
			if (!Compare((int) KnownPosition.Z, (int) other.KnownPosition.Z, 4)) return false;
			if (!bbox.Intersects(other.GetBoundingBox())) return false;

			return true;
		}
		
		public double DistanceToHorizontal(IEntity entity)
		{
			if (entity == null) return -1;
			return Vector2.Distance(KnownPosition, entity.KnownPosition);
		}

		public double DistanceTo(IEntity entity)
		{
			if (entity == null) return -1;
			return Vector3.Distance(KnownPosition, entity.KnownPosition);
		}
		
		private HashSet<IEntity> SeenEntities   { get; set; } = new HashSet<IEntity>();
		private HashSet<IEntity> UnseenEntities { get; set; } = new HashSet<IEntity>();
		public virtual bool CanSee(IEntity target)
		{
			if (SeenEntities.Contains(target)) return true;
			if (UnseenEntities.Contains(target)) return false;

			Vector3 entityPos = KnownPosition + new Vector3(0, (float) (this is Player ? 1.62f : Height), 0);
			Vector3 targetPos = target.KnownPosition + new Vector3(0, (float) (target is Player ? 1.62f : target.Height), 0);
			float distance = Vector3.Distance(entityPos, targetPos);

			Vector3 rayPos    = entityPos;
			var     direction = Vector3.Normalize(targetPos - entityPos);

			if (distance < direction.Length())
			{
				UnseenEntities.Add(target);
				return true;
			}

			do
			{
				if (Level.GetBlock(rayPos).Solid)
				{
					//Log.Debug($"{GetType()} can not see target");
					//BroadcastEntityEvent();
					UnseenEntities.Add(target);
					return false;
				}

				//var particle = new DustParticle(Level, Color.AntiqueWhite);
				//particle.Position = rayPos;
				//particle.Spawn();

				rayPos += direction;
			} while (distance > Vector3.Distance(entityPos, rayPos));

			SeenEntities.Add(target);
			return true;
		}
		
		private bool Compare(int a, int b, int m)
		{
			a = a >> m;
			b = b >> m;
			return a == b || a == b - 1 || a == b + 1;
		}
		
		public virtual void RenderNametag(IRenderArgs renderArgs)
		{
			string clean = NameTag;

			if (string.IsNullOrWhiteSpace(clean))
				return;
			
			var halfWidth = -((((float) Width) * Scale));
			
			var maxDistance = (renderArgs.Camera.FarDistance) / (64f);

			Vector3 posOffset = new Vector3(0, 0.25f, 0);

			if (RenderEntity && ModelRenderer != null && ModelRenderer.Valid && !IsInvisible)
			{
				posOffset.Y += (float) (Height * Scale);
			}

			var cameraPosition = new Vector3(renderArgs.Camera.Position.X, 0, renderArgs.Camera.Position.Z);
			
			var rotation = new Vector3(KnownPosition.X, 0, KnownPosition.Z) - cameraPosition;
			rotation.Normalize();
			
			
			var pos = KnownPosition + posOffset + (rotation * halfWidth);
			//pos.Y = 0;
			
			var distance = Vector3.Distance(pos, renderArgs.Camera.Position);
			if (distance >= maxDistance)
			{
				return;
			}

			Vector2 textPosition;
			
			var screenSpace = renderArgs.GraphicsDevice.Viewport.Project(pos, 
				renderArgs.Camera.ProjectionMatrix,
				renderArgs.Camera.ViewMatrix,
				Matrix.Identity);

			textPosition.X = screenSpace.X;
			textPosition.Y = screenSpace.Y;

			float depth = screenSpace.Z;

			var scaleRatio = (1.0f / depth);
			//var scaleRatio = Alex.Instance.GuiRenderer.ScaledResolution.ScaleFactor;
			//scale = 0.5f;
			float scaler = NametagScale - (distance * (NametagScale / maxDistance));
			//float scaler = NametagScale;
			var scale = new Vector2(scaler * scaleRatio, scaler * scaleRatio);
			//scale *= Alex.Instance.GuiRenderer.ScaledResolution.ElementScale;

			Vector2 renderPosition = textPosition;
			int yOffset = 0;
			foreach (var str in clean.Split('\n'))
			{
				var line = str.Trim();
				var stringCenter = Alex.Font.MeasureString(line, scale);
				var c            = new Point((int) stringCenter.X, (int) stringCenter.Y);

				renderPosition.X = (int) (textPosition.X - (c.X / 2d));
				renderPosition.Y = (int) (textPosition.Y - (c.Y / 2d)) + yOffset;

				renderArgs.SpriteBatch.FillRectangle(
					new Rectangle(renderPosition.ToPoint(), c), new Color(Color.Black, 128), screenSpace.Z);

				Alex.Font.DrawString(
					renderArgs.SpriteBatch, line, renderPosition, TextColor.White, FontStyle.None, scale,
					layerDepth: screenSpace.Z);

				yOffset += c.Y;
			}
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
		
		public static float NametagScale { get; set; } = 2f;
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