using System;
using System.Collections;
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
using Alex.Entities.Properties;
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

		public World Level { get; set; }

		public int JavaEntityId { get; protected set; }
		public int EntityTypeId { get; protected set; }
		public long EntityId { get; set; }
		public bool IsSpawned { get; set; }

		public DateTime LastUpdatedTime { get; set; }
		public virtual PlayerLocation KnownPosition { get; set; }

		public Vector3 Velocity { get; set; } = Vector3.Zero;
		private float _posOffset = 0;
		private bool _posOffsetSet = false;

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

		public bool AboveWater { get; set; } = false;
		public bool HeadInWater { get; set; } = false;
		public bool FeetInWater { get; set; } = false;
		public bool IsInWater { get; set; } = false;
		public bool IsInLava { get; set; } = false;
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
		public double Width { get; set; } = 1;
		public double Length => Width;
		public double Drag { get; set; } = 0.02f;// 8f;

		public double Gravity { get; set; } = 0.08f; //16.8f; //9.81f; //1.6f;
		//Drag & gravity etc is Vanilla * 400
		
		public float TerminalVelocity { get; set; } = 78.4f;
		
		public UUID UUID { get; set; } = new UUID(Guid.Empty.ToByteArray());

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

		private EntityModelRenderer.ModelBone _leftLegModel;
		private EntityModelRenderer.ModelBone _rightLegModel;

		private EntityModelRenderer.ModelBone _leftSleeveModel;
		private EntityModelRenderer.ModelBone _rightSleeveModel;

		private EntityModelRenderer.ModelBone _leftPantsModel;
		private EntityModelRenderer.ModelBone _rightPantsModel;

		private EntityModelRenderer.ModelBone _jacketModel;
		private EntityModelRenderer.ModelBone _body;
		private EntityModelRenderer.ModelBone _head;
		
		public HealthManager HealthManager { get; }
		private Dictionary<string, EntityProperty> _entityProperties = new Dictionary<string, EntityProperty>()
		{
			
		};

		public IReadOnlyDictionary<string, EntityProperty> EntityProperties
		{
			get => _entityProperties;
			//set => _entityProperties = value;
		}

		public double BaseMovementSpeed { get; set; }
		public Entity(int entityTypeId, World level, NetworkProvider network)
		{
			Network = network;

            EntityId = -1;
            Level = level;
            EntityTypeId = entityTypeId;
            KnownPosition = new PlayerLocation();
            Inventory = new Inventory(46);
            //	HealthManager = new HealthManager(this);

            Inventory.SlotChanged += OnInventorySlotChanged;
            Inventory.SelectedHotbarSlotChanged += InventoryOnSelectedHotbarSlotChanged;

			HideNameTag = true;
			ServerEntity = true;
			IsAffectedByGravity = true;
			
			HealthManager = new HealthManager(this);
			UUID = new UUID(Guid.NewGuid().ToByteArray());
			
			BaseMovementSpeed = 4.317D;
			
			AddOrUpdateProperty(new FlyingSpeedProperty(this));
			AddOrUpdateProperty(new MovementSpeedProperty(this));
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
			if (!_entityProperties.TryAdd(property.Key, property))
			{
				_entityProperties[property.Key] = property;
			}
		}

		public double CalculateMovementSpeed()
		{
			var baseMovementSpeed = BaseMovementSpeed / 2D;

			var modifier =
				(_entityProperties[
					IsFlying ? Networking.Java.Packets.Play.EntityProperties.FlyingSpeed :
						Networking.Java.Packets.Play.EntityProperties.MovementSpeed]).Calculate();

			return baseMovementSpeed * modifier;
		}

		private void ScaleChanged()
		{
			if (ModelRenderer != null)
			{
				ModelRenderer.Scale = _scale;
			}

			//if (ItemRenderer != null)
			//	ItemRenderer.Scale = new Vector3(_scale);
		}
		
		public void SetInventory(Inventory inventory)
		{
			Inventory = inventory;
			//	HealthManager = new HealthManager(this);
			
			Inventory.SlotChanged += OnInventorySlotChanged;
			Inventory.SelectedHotbarSlotChanged += InventoryOnSelectedHotbarSlotChanged;
		}

        private void CheckHeldItem()
        {
            var inHand = Inventory.MainHand;
            //Log.Info($"Inventory slot changed.");

            if ((inHand == null || inHand.Count == 0 || inHand.Id <= 0) && ItemRenderer != null)
            {
              //  if (ModelRenderer.GetBone("rightItem", out EntityModelRenderer.ModelBone bone))
                {
               //     _rightArmModel?.Detach(ItemRenderer);
                }

                ItemRenderer = null;
                return;
            }

            if (inHand != null)
            {
	            if (!string.IsNullOrWhiteSpace(inHand.Name))
	            {
		            /*var itemModel = Alex.Instance.Resources.ResourcePack.ItemModels.FirstOrDefault(x =>
		                x.Key.Contains(inHand.Name, StringComparison.InvariantCultureIgnoreCase));
		            
		            ItemRenderer = new ItemModelRenderer(itemModel.Value, Alex.Instance.Resources.ResourcePack);*/

		            var renderer = inHand.Renderer.Clone();
		            if (renderer == null)
		            {
			            Log.Warn($"No renderer for item: {inHand.Name}");
			            return;
		            }

		            if (renderer == ItemRenderer)
			            return;

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
                    //if (ModelRenderer.GetBone("rightItem", out EntityModelRenderer.ModelBone bone))
                  //  {
	               //     _rightArmModel?.Detach(ItemRenderer);
               //     }

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
		internal DateTime LastTickTime { get; set; } = DateTime.UtcNow;
		
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
							BitArray bits = new BitArray(BitConverter.GetBytes(lng.Value));
							HandleEntityFlags(bits);
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

		private void HandleEntityFlags(BitArray bits)
		{
			if ((this is Player))
				return;
			
			IsInvisible = bits[(int) MiNET.Entities.Entity.DataFlags.Invisible];
			IsSneaking = bits[(int) MiNET.Entities.Entity.DataFlags.Sneaking];
			IsOnFire = bits[(int) MiNET.Entities.Entity.DataFlags.OnFire];
			IsSprinting = bits[(int) MiNET.Entities.Entity.DataFlags.Sprinting];
			
			NoAi = bits[(int) MiNET.Entities.Entity.DataFlags.NoAi];

			IsAlwaysShowName = bits[(int) MiNET.Entities.Entity.DataFlags.AlwaysShowName];
			IsBaby = bits[(int) MiNET.Entities.Entity.DataFlags.Baby];
			IsUsingItem = bits[(int) MiNET.Entities.Entity.DataFlags.UsingItem];
			//HideNameTag = !bits[(int) MiNET.Entities.Entity.DataFlags.ShowName];
			IsAngry = bits[(int) MiNET.Entities.Entity.DataFlags.Angry];
			IsInLove = bits[(int) MiNET.Entities.Entity.DataFlags.InLove];
			IsRiding = bits[(int) MiNET.Entities.Entity.DataFlags.Riding];
			IsTempted = bits[(int) MiNET.Entities.Entity.DataFlags.Tempted];
			IsTamed = bits[(int) MiNET.Entities.Entity.DataFlags.Tamed];
			IsLeashed = bits[(int) MiNET.Entities.Entity.DataFlags.Leashed];
			IsSheared = bits[(int) MiNET.Entities.Entity.DataFlags.Sheared];
			IsChested = bits[(int) MiNET.Entities.Entity.DataFlags.Chested];
			HasCollision = bits[(int) MiNET.Entities.Entity.DataFlags.HasCollision];
			IsAffectedByGravity = bits[(int) MiNET.Entities.Entity.DataFlags.AffectedByGravity];
			IsFlagAllFlying = bits[(int) MiNET.Entities.Entity.DataFlags.FlagAllFlying];
			IsSilent = bits[(int) MiNET.Entities.Entity.DataFlags.Silent];
			IsSitting = bits[(int) MiNET.Entities.Entity.DataFlags.Sitting];
			IsWallClimbing = bits[(int) MiNET.Entities.Entity.DataFlags.WallClimbing];
			IsResting = bits[(int) MiNET.Entities.Entity.DataFlags.Resting];
			//IsBreathing = bits[(int) MiNET.Entities.Entity.DataFlags.Breathing];
		}
		
		/// <inheritdoc />
		public long RenderedVertices { get; private set; }

		public virtual void Render(IRenderArgs renderArgs)
		{

			long rendered = 0;

			if ((RenderEntity && !IsInvisible) || ShowItemInHand)
			{
				ModelRenderer.Render(renderArgs, KnownPosition, !RenderEntity || IsInvisible);

				if (ModelRenderer.Valid)
				{
					rendered += ModelRenderer.Vertices;
				}
			}
			if (ShowItemInHand && ItemRenderer != null)
			{
				ItemRenderer.Render(renderArgs);
				
				rendered += ItemRenderer.VertexCount;
			}

			RenderedVertices = rendered;
		}

		public virtual void Update(IUpdateArgs args)
		{
			var now = DateTime.UtcNow;

            if ((RenderEntity && !IsInvisible) || ShowItemInHand)
            {
                ModelRenderer.Update(args, KnownPosition);

                CalculateLegMovement(args);
                
                if (ShowItemInHand)
                {
	                ItemRenderer?.Update(Matrix.Identity *
	                                     Matrix.CreateScale(Scale) *
	                                     Matrix.CreateRotationY(MathHelper.ToRadians(180f - KnownPosition.HeadYaw)) *
	                                     Matrix.CreateTranslation(KnownPosition.X, KnownPosition.Y, KnownPosition.Z), new PlayerLocation(KnownPosition.X, KnownPosition.Y, KnownPosition.Z, 180f - KnownPosition.HeadYaw, 180f - KnownPosition.Yaw, KnownPosition.Pitch));

                    ItemRenderer?.Update(args.GraphicsDevice, args.Camera);
                }
            }
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
		public float   DistanceMoved { get; set; } = 0;

		internal double   _timeStoppedMoving = 0;
		private  float    _mvSpeed           = 0f;

		protected bool ServerEntity { get; set; } = true;

		public void SwingArm(bool broadcast = false)
		{
			if (_rightArmModel != null)
			{
				_rightArmModel.Animations.Enqueue(new SwingAnimation(_rightArmModel, TimeSpan.FromMilliseconds(300)));
			}

			if (broadcast)
			{
				Network.PlayerAnimate(PlayerAnimations.SwingRightArm);
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
				var posOffset = new Vector3(0f, -1.5f, -4.5f);

				if (!_body.IsAnimating)
				{
					_body.Rotation = new Vector3(-25f, _body.Rotation.Y, _body.Rotation.Z);
					_body.Position = posOffset;
				}

				//_head.Position = new Vector3(_body.Position.X, 0.25f, 0f);
				if (_rightArmModel != null && _leftArmModel != null)
				{
					if (!_leftArmModel.IsAnimating)
					{
						_leftArmModel.Rotation = new Vector3(-20f, 0f, 0f);
						
						_leftArmModel.Position = posOffset;
					}

					if (!_rightArmModel.IsAnimating)
					{
						_rightArmModel.Rotation = new Vector3(-20f, 0f, 0f);
						_rightArmModel.Position = posOffset;
					}
				}

				if (_head != null && !_head.IsAnimating)
				{
					_head.Position = posOffset;
				}
			}
			else if (_body != null && !IsSneaking)
			{
				if (!_body.IsAnimating)
				{
					_body.Position = Vector3.Zero;
					_body.Rotation = new Vector3(0f);
				}

				if (_rightArmModel != null && _leftArmModel != null && !_rightArmModel.IsAnimating && !_leftArmModel.IsAnimating)
				{
					_rightArmModel.Position = _leftArmModel.Position = Vector3.Zero;
				}

				if (_head != null && !_head.IsAnimating)
				{
					_head.Position = Vector3.Zero;
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

		private DateTime NextUpdate     = DateTime.MinValue;
		private DateTime PreviousUpdate = DateTime.MinValue;
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

			if (ServerEntity && DateTime.UtcNow >= NextUpdate)
			{
				var distanceMoved = DistanceMoved;
				DistanceMoved = 0;

				//PreviousUpdate
				_mvSpeed = (float) (distanceMoved * (TimeSpan.FromSeconds(1) / (DateTime.UtcNow - PreviousUpdate)));
				
				PreviousUpdate = DateTime.UtcNow;
				NextUpdate = DateTime.UtcNow + TimeSpan.FromMilliseconds(500);
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
				if (headBlock.BlockMaterial == Material.Water || headBlock.IsWater)
				{
					HeadInWater = true;
				}
				else
				{
					HeadInWater = false;
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
					IsInLava = true;
				}
				else
				{
					IsInLava = false;
				}

				/*if (!feetBlock.Any(x => x.Storage == 0 && x.State.Block.Solid))
				{
					KnownPosition.OnGround = false;
				}
				else
				{
					KnownPosition.OnGround = true;
				}*/
			}

			IsInWater = FeetInWater || HeadInWater;

			/*
			if (headBlock != null)
			{
				if (headBlock.IsWater)
				{
					IsInWater = true;
				}
				else
				{
					IsInWater = false;
				}
			}*/

			//HealthManager.OnTick();
		}

		private float HeadRenderOffset = 0;
		private int turnTicks;
		private int turnTicksLimit = 10;
		private float lastRotationYawHead = 0f;
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
			const float MOVEMENT_THRESHOLD_SQ = 2.5f;
			// if moving:
			// 1) snap the body yaw (renderYawOffset) to the movement direction (rotationYaw)
			// 2) constrain the head yaw (rotationYawHead) to be within +/- 90 of the body yaw (renderYawOffset)
			if (distSQ > MOVEMENT_THRESHOLD_SQ)
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

				lastRotationYawHead = newRotationYawHead;
				turnTicks = 0;
			}
			else
			{
				var changeInHeadYaw = Math.Abs(KnownPosition.HeadYaw - lastRotationYawHead);
				if (changeInHeadYaw > 15f)
				{
					turnTicks = 0;
					lastRotationYawHead = KnownPosition.HeadYaw;
				}
				else
				{
					turnTicks++;
					if (turnTicks > turnTicksLimit)
					{
						maximumHeadBodyAngleDifference =
							Math.Max(1f - (float) ((float)(turnTicks - turnTicksLimit) / turnTicksLimit), 0f) *
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

			ModelRenderer.GetBone("rightLeg", out _rightLegModel);
			ModelRenderer.GetBone("leftLeg", out _leftLegModel);

			ModelRenderer.GetBone("rightSleeve", out _rightSleeveModel);
			ModelRenderer.GetBone("leftSleeve", out _leftSleeveModel);

			ModelRenderer.GetBone("rightPants", out _rightPantsModel);
			ModelRenderer.GetBone("leftPants", out _leftPantsModel);

			ModelRenderer.GetBone("jacket", out _jacketModel);

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
		
		public BoundingBox BoundingBox => GetBoundingBox();
		public virtual BoundingBox GetBoundingBox()
		{
			var pos = KnownPosition;
			return GetBoundingBox(pos);
		}
		
		public virtual BoundingBox GetBoundingBox(Vector3 pos)
		{
			double halfWidth = (Width * Scale) / 2D;

			return new BoundingBox(new Vector3((float)(pos.X - halfWidth), pos.Y, (float)(pos.Z - halfWidth)), new Vector3((float)(pos.X + halfWidth), (float)(pos.Y + (Height * Scale)), (float)(pos.Z + halfWidth)));
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
		
		public void RenderNametag(IRenderArgs renderArgs)
		{
			string clean = NameTag;

			if (string.IsNullOrWhiteSpace(clean))
				return;
			
			var halfWidth = -((((float) Width) * Scale));
			
			var maxDistance = (renderArgs.Camera.FarDistance) / (64f);

			Vector3 posOffset = new Vector3(0, 0.25f, 0);

			if (RenderEntity && ModelRenderer != null && ModelRenderer.Valid && !IsInvisible && !ModelRenderer.Texture.IsFullyTransparent)
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

		public virtual void CollidedWithWorld(Vector3 direction, Vector3 position)
		{
			if (this is Player p)
			{
				 // Vector3.Distance(KnownPosition, position);
			//	Log.Info($"Collided with world! CollisionPoint: {position} Distance: {distanceToCollision} Direction: {direction} Velocity: {Velocity}");
			}

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
				if (entityDescription.Textures.TryGetValue(texture, out texture) && entityDescription.Geometry.TryGetValue(geometry, out var geometryName))
				{
					if (ModelFactory.TryGetModel(geometryName, out var newModel))
					{
						Alex.Instance.UIThreadQueue.Enqueue(
							() =>
							{
								if (Alex.Instance.Resources.BedrockResourcePack.TryGetTexture(
									texture, out var newTexture))
								{
									ModelRenderer = new EntityModelRenderer(newModel, TextureUtils.BitmapToTexture2D(
										Alex.Instance.GraphicsDevice, newTexture));
									//ModelRenderer?.Texture = TextureUtils.BitmapToTexture2D(
									//	Alex.Instance.GraphicsDevice, newTexture);
								}
							});

						return true;
					}
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
						Alex.Instance.UIThreadQueue.Enqueue(
							() =>
							{
								//var texture = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, newTexture);
								ModelRenderer.Texture = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, newTexture);
								/*ModelRenderer = new EntityModelRenderer(
									newModel,);*/

								//ModelRenderer?.Texture = TextureUtils.BitmapToTexture2D(
								//	Alex.Instance.GraphicsDevice, newTexture);
							});

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
			ModelRenderer?.Dispose();
		}
	}
}