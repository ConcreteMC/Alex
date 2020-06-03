using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Alex.API;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.Graphics.Models.Items;
using Alex.Net;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Java;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using MathF = System.MathF;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Entities
{
	public class Entity : IEntity, IPhysicsEntity
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Entity));

		private EntityModelRenderer _modelRenderer;

		internal EntityModelRenderer ModelRenderer
		{
			get
			{
				return _modelRenderer;
			}
			set
			{
				_modelRenderer = value;
				UpdateModelParts();
			}
		}

		public World Level { get; set; }

		public int JavaEntityId { get; protected set; }
		public int EntityTypeId { get; private set; }
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
		public bool HideNameTag { get; set; } = true;
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
		public double Length { get; set; } = 1;
		public double Drag { get; set; } = 8f;
		public double Gravity { get; set; } = 16.8f; //9.81f; //1.6f;
		//Drag & gravity etc is Vanilla * 400
		
		public float TerminalVelocity { get; set; } = 78.4f;

		public float MovementSpeedModifier { get; set; } = 0.1f;
		public double BaseMovementSpeed { get; set; } = 3;
		public double MovementSpeed { get; set; } = 0.1F;
		public double FlyingSpeed { get; set; } = 0.4F;
		
		public int Data { get; set; }
		public UUID UUID { get; set; } = new UUID(Guid.Empty.ToByteArray());

		public bool CanFly { get; set; } = false;
		public bool IsFlying { get; set; } = false;

		public bool IsCollidingWithWorld { get; set; } = false;

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
            var inHand = Inventory[Inventory.SelectedSlot];
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

		           // renderer.Scale = new Vector3(_scale);

		            ItemRenderer = renderer;
					
		            if (this is Player p)
		            {
			            var pos = renderer.DisplayPosition;
			            //if (pos.HasFlag(DisplayPosition.FirstPerson) || pos.HasFlag(DisplayPosition.ThirdPerson))
			            {
				            if (p.IsLeftyHandy)
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
		public bool IsSaddled { get; set; }
		public bool IsPowered { get; set; }
		public bool IsIgnited { get; set; }
		public bool IsBaby { get; set; }
		public bool IsConverting { get; set; }
		public bool IsCritical { get; set; }
		public bool IsShowName => !HideNameTag;
		public bool IsAlwaysShowName { get; set; }
		public bool IsNoAi => NoAi;
		public bool IsSilent { get; set; }
		public bool IsWallClimbing { get; set; }
		public bool IsResting { get; set; }
		public bool IsSitting { get; set; }
		public bool IsAngry { get; set; }
		public bool IsInterested { get; set; }
		public bool IsCharged { get; set; }
		public bool IsTamed { get; set; }
		public bool IsLeashed { get; set; }
		public bool IsSheared { get; set; }
		public bool IsFlagAllFlying { get; set; }
		public bool IsElder { get; set; }
		public bool IsMoving { get; set; }
		public bool IsBreathing => !IsInWater;
		public bool IsChested { get; set; }
		public bool IsStackable { get; set; }
		public bool HasCollision { get; set; } = true;
		public bool IsAffectedByGravity { get; set; } = true;
		
		public bool RenderEntity { get; set; } = true;
		public bool ShowItemInHand { get; set; } = false;

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
							Length = fltw.Value;
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
			IsInvisible = bits[(int) MiNET.Entities.Entity.DataFlags.Invisible];
			IsSneaking = bits[(int) MiNET.Entities.Entity.DataFlags.Sneaking];
			IsOnFire = bits[(int) MiNET.Entities.Entity.DataFlags.OnFire];
			IsSprinting = bits[(int) MiNET.Entities.Entity.DataFlags.Sprinting];
			NoAi = bits[(int) MiNET.Entities.Entity.DataFlags.NoAi];
			IsAlwaysShowName = bits[(int) MiNET.Entities.Entity.DataFlags.AlwaysShowName];
			IsBaby = bits[(int) MiNET.Entities.Entity.DataFlags.Baby];
			IsUsingItem = bits[(int) MiNET.Entities.Entity.DataFlags.UsingItem];
			HideNameTag = !bits[(int) MiNET.Entities.Entity.DataFlags.ShowName];
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

			if (RenderEntity || ShowItemInHand)
			{
				ModelRenderer.Render(renderArgs, KnownPosition, !RenderEntity);

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

            if (RenderEntity || ShowItemInHand)
            {
                ModelRenderer.Update(args, KnownPosition);

                CalculateLegMovement(args);
                
                if (ShowItemInHand)
                {
                    //CheckHeldItem();

                    //Matrix.CreateRotationY(MathUtils.ToRadians((-KnownPosition.HeadYaw)))
                    //			ItemRenderer?.Update(Matrix.CreateRotationY(MathUtils.ToRadians(180f - KnownPosition.HeadYaw)) * Matrix.CreateTranslation(KnownPosition));
                 //   ItemRenderer?.Update(null, new PlayerLocation(KnownPosition.X, KnownPosition.Y, KnownPosition.Z, 180f - KnownPosition.HeadYaw, 180f - KnownPosition.Yaw, KnownPosition.Pitch));

                 if (_rightArmModel != null && ItemRenderer != null)
                 {
	                 //ItemRenderer.Rotation = _rightArmModel.Rotation;
                 }

                 ItemRenderer?.Update(Matrix.Identity *
                                      Matrix.CreateScale(Scale) *
                                      Matrix.CreateRotationY(MathHelper.ToRadians(180f - KnownPosition.HeadYaw)) *
                                      Matrix.CreateTranslation(KnownPosition.X, KnownPosition.Y, KnownPosition.Z), new PlayerLocation(KnownPosition.X, KnownPosition.Y, KnownPosition.Z, 180f - KnownPosition.HeadYaw, 180f - KnownPosition.Yaw, KnownPosition.Pitch));
                    //ItemRenderer?.World = 
                    ItemRenderer?.Update(args.GraphicsDevice, args.Camera);
                }
            }

      /*      if (now.Subtract(LastUpdatedTime).TotalMilliseconds >= 50)
            {
                LastUpdatedTime = now;
                try
                {
                    OnTick();
                }
                catch (Exception e)
                {
                    Log.Warn(e, $"Exception while trying to tick entity!");
                }
            }*/
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
		internal float   DistanceMoved { get; set; } = 0;

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

			if (IsSneaking && _body != null)
			{
				var posOffset = new Vector3(0f, -1.5f, -4.5f);
				_body.Rotation = new Vector3(-25f, _body.Rotation.Y, _body.Rotation.Z);
				_body.Position = posOffset;

				//_head.Position = new Vector3(_body.Position.X, 0.25f, 0f);
				if (_rightArmModel != null && _leftArmModel != null)
				{
					_leftArmModel.Rotation = new Vector3(-20f, 0f, 0f);
					_leftArmModel.Position = posOffset;


					if (!_rightArmModel.IsAnimating)
					{
						_rightArmModel.Rotation = new Vector3(-20f, 0f, 0f);
					}

					_rightArmModel.Position = posOffset;
					
					if (_rightSleeveModel != null && _leftSleeveModel != null)
					{
						_rightSleeveModel.Rotation = _rightArmModel.Rotation;
						_rightSleeveModel.Position = posOffset;
						
						_leftSleeveModel.Rotation = _leftArmModel.Rotation;
						_leftSleeveModel.Position = posOffset;
					}
				}

				if (_head != null)
				{
					_head.Position = posOffset;
				}
			}
			else if (_body != null && !IsSneaking)
			{
				_body.Position = Vector3.Zero;
				_body.Rotation = new Vector3(0f);

				if (_rightArmModel != null && _leftArmModel != null)
				{
					_rightArmModel.Position = _leftArmModel.Position = Vector3.Zero;
					
					if (_rightSleeveModel != null && _leftSleeveModel != null)
					{
						_rightSleeveModel.Rotation = _rightArmModel.Rotation;
						_rightSleeveModel.Position = Vector3.Zero;
						
						_leftSleeveModel.Rotation = _leftArmModel.Rotation;
						_leftSleeveModel.Position = Vector3.Zero;
					}
				}

				if (_head != null)
				{
					_head.Position = Vector3.Zero;
				}
			}

			var moveSpeed = MovementSpeed * 20f;
			var tcos0     = (float) (Math.Cos(distance * (38.17 * 20f)) * moveSpeed) * (57.3f * 20f);
			var tcos1     = -tcos0;

			//_armRotation = _armRotation;

			// Test arm rotations
			if (!IsSneaking && _leftArmModel != null && _rightArmModel != null)
			{
				//var lArmRot = new Vector3((0.5f + MathF.Sin(_armRotation)) * 7.5f, 0f,
				//	0.1f + (MathF.Cos(_armRotation) * 1.5f));
				Vector3 rArmRot = Vector3.Zero;
				var     lArmRot = new Vector3(tcos0, 0, 0);

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


				_leftArmModel.Rotation = rArmRot;

				if (!_rightArmModel.IsAnimating)
				{
					_rightArmModel.Rotation = -rArmRot;
				}

				if (_rightSleeveModel != null && _leftSleeveModel != null)
				{
					_rightSleeveModel.Rotation = -rArmRot;
					_leftSleeveModel.Rotation = rArmRot;
				}
			}


			if (_leftLegModel != null && _rightLegModel != null)
			{
				Vector3 lLegRot = Vector3.Zero;
				Vector3 rLegRot = Vector3.Zero;

				if (distSQ > 0f)
				{
					_legRotation += (float) (_mvSpeed) * dt;
					;

					lLegRot = new Vector3(MathF.Sin(_legRotation) * 34.5f, 0f, 0f);
					rLegRot = new Vector3(-MathF.Sin(_legRotation) * 34.5f, 0f, 0f);
					//lLegRot = new Vector3(tcos0, 0f, 0f);
					//rLegRot = new Vector3(tcos1, 0f, 0f);
				}
				else
				{
					_legRotation = 0f;
				}

				_leftLegModel.Rotation = lLegRot;
				_rightLegModel.Rotation = rLegRot;

				if (_leftPantsModel != null && _rightPantsModel != null)
				{
					_leftPantsModel.Rotation = lLegRot;
					_rightPantsModel.Rotation = rLegRot;
				}
			}


			var itemRender = ItemRenderer;
			var rightArm = _rightArmModel;
			if (itemRender != null && rightArm != null)
			{
				
			}
			
			_prevUpdatePosition = pos;
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

		if (!(Network is JavaNetworkProvider))
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

				if (!feetBlock.Any(x => x.Storage == 0 && x.State.Block.Solid))
				{
					KnownPosition.OnGround = false;
				}
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

		protected void UpdateModelParts()
		{
			if (ModelRenderer == null)
				return;

			ScaleChanged();
			
			ModelRenderer.GetBone("body", out _body);
			ModelRenderer.GetBone("leftArm", out _rightArmModel);
			ModelRenderer.GetBone("rightArm", out _leftArmModel);

			ModelRenderer.GetBone("rightLeg", out _leftLegModel);
			ModelRenderer.GetBone("leftLeg", out _rightLegModel);

			ModelRenderer.GetBone("rightSleeve", out _leftSleeveModel);
			ModelRenderer.GetBone("leftSleeve", out _rightSleeveModel);

			ModelRenderer.GetBone("rightPants", out _leftPantsModel);
			ModelRenderer.GetBone("leftPants", out _rightPantsModel);

			ModelRenderer.GetBone("jacket", out _jacketModel);
			ModelRenderer.GetBone("head", out _head);

			if (ModelRenderer.GetBone("hat", out EntityModelRenderer.ModelBone hat))
			{
				foreach (var c in hat.Parts)
				{
					c.ApplyHeadYaw = true;
					c.ApplyYaw = false;
					c.ApplyPitch = true;
				}
			}
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
				var stringCenter = Alex.Font.MeasureString(str, scale);
				var c            = new Point((int) stringCenter.X, (int) stringCenter.Y);

				renderPosition.X = (int) (textPosition.X - (c.X / 2d));
				renderPosition.Y = (int) (textPosition.Y - (c.Y / 2d)) + yOffset;

				renderArgs.SpriteBatch.FillRectangle(
					new Rectangle(renderPosition.ToPoint(), c), new Color(Color.Black, 128), screenSpace.Z);

				Alex.Font.DrawString(
					renderArgs.SpriteBatch, str, renderPosition, TextColor.White, FontStyle.None, scale,
					layerDepth: screenSpace.Z);

				yOffset += c.Y;
			}
		}

		public static float NametagScale { get; set; } = 2f;
		public void Dispose()
		{
			ModelRenderer?.Dispose();
		}
	}
}