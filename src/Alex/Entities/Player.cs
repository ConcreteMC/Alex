using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Input;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Graphics.Camera;
using Alex.Items;
using Alex.Net;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Bedrock;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using ContainmentType = Microsoft.Xna.Framework.ContainmentType;
using IBlockState = Alex.API.Blocks.State.IBlockState;
using Inventory = Alex.Utils.Inventory;
using MathF = System.MathF;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using Skin = Alex.API.Utils.Skin;

namespace Alex.Entities
{
    public class Player : PlayerMob
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Player));

        public static readonly float EyeLevel = 1.625F;
        public static readonly float Height = 1.8F;

		public PlayerIndex PlayerIndex { get; }

		public float FOVModifier { get; set; } = 0;

		public PlayerController Controller { get; }
        public Vector3 Raytraced = Vector3.Zero;
        public Vector3 AdjacentRaytrace = Vector3.Zero;
        public bool HasAdjacentRaytrace = false;
        public bool HasRaytraceResult = false;

        public bool IsFirstPersonMode { get; set; } = false;
        public bool IsLeftyHandy { get; set; } = false;
        
        public bool IsWorldImmutable { get; set; } = false;
        public bool IsNoPvP { get; set; } = true;
        public bool IsNoPvM { get; set; } = true;
        
        private World World { get; }
        public Camera Camera { get; internal set; }
        public Player(GraphicsDevice graphics, InputManager inputManager, string name, World world, Skin skin, NetworkProvider networkProvider, PlayerIndex playerIndex, Camera camera) : base(name, world, networkProvider, skin.Texture)
        {
	        World = world;
		//	DoRotationCalculations = false;
			PlayerIndex = playerIndex;
		    Controller = new PlayerController(graphics, world, inputManager, this, playerIndex);
		    Camera = camera;
		    NoAi = false;

			//Inventory = new Inventory(46);
			//Inventory.SelectedHotbarSlotChanged += SelectedHotbarSlotChanged;
			//base.Inventory.IsPeInventory = true;
			MovementSpeedModifier = 1f;
			MovementSpeed = 4.306f;
			BaseMovementSpeed = 4.317;
			FlyingSpeed = 10.89f;

			SnapHeadYawRotationOnMovement = false;
			SnapYawRotationOnMovement = true;
			DoRotationCalculations = false;
			
			RenderEntity = true;
			ShowItemInHand = true;

			ServerEntity = false;
        }

        /// <inheritdoc />
        public override bool NoAi { get; set; }

        protected override void OnInventorySlotChanged(object sender, SlotChangedEventArgs e)
        {
	        //Crafting!
	        if (e.Index >= 41 && e.Index <= 44)
	        {
		        McpeInventoryTransaction transaction = McpeInventoryTransaction.CreateObject();
		        transaction.transaction = new NormalTransaction()
		        {
			        TransactionRecords = new List<TransactionRecord>()
			        {
				        new CraftTransactionRecord()
				        {
					        Action = McpeInventoryTransaction.CraftingAction.CraftAddIngredient,
					        Slot = e.Index,
					        NewItem = BedrockClient.GetMiNETItem(e.Value),
					        OldItem = BedrockClient.GetMiNETItem(e.OldItem)
				        }
			        }
		        };
	        }
	        
	        base.OnInventorySlotChanged(sender, e);
        }

        public bool IsBreakingBlock => _destroyingBlock;

	    public float BlockBreakProgress
	    {
		    get
		    {
			    if (!IsBreakingBlock)
				    return 0;
			    
			    var end = DateTime.UtcNow;
			    var start = _destroyingTick;

			    var timeRan = (end - start).TotalMilliseconds / 50d;

			    return (float) ((1f / (float) _destroyTimeNeeded) * timeRan);
		    }
	    }

	    public double BreakTimeNeeded
	    {
		    set
		    {
			    _destroyTimeNeeded = value;
		    }
	    }

	    public bool WaitingOnChunk { get; set; } = false;
	    
	    public BlockCoordinates TargetBlock => _destroyingTarget;

	    private BlockCoordinates _destroyingTarget = BlockCoordinates.Zero;
	    private bool _destroyingBlock = false;
        private DateTime _destroyingTick = DateTime.MaxValue;
	    private double _destroyTimeNeeded = 0;
	    private BlockFace _destroyingFace;
	    
	    private int PreviousSlot { get; set; } = 9;
	    private DateTime _lastTimeWithoutInput = DateTime.MinValue;
	    private bool _prevCheckedInput = false;
	    private DateTime _lastAnimate = DateTime.MinValue;

	    public bool CanSprint => HealthManager.Hunger > 6;

		public override void Update(IUpdateArgs args)
		{
			if (WaitingOnChunk && Age % 4 == 0)
			{
				NoAi = true;
				
				if (Level.GetChunk(KnownPosition.GetCoordinates3D(), true) != null)
				{
					Velocity = Vector3.Zero;
					WaitingOnChunk = false;
					NoAi = false;
				}
			}

			ChunkCoordinates oldChunkCoordinates = new ChunkCoordinates(base.KnownPosition);
			bool sprint = IsSprinting;
			bool sneak = IsSneaking;

			if (!CanFly && IsFlying)
				IsFlying = false;

			if (IsSprinting && !CanSprint)
			{
				IsSprinting = false;
			}
			
			Controller.Update(args.GameTime);
			//KnownPosition.HeadYaw = KnownPosition.Yaw;

			if (IsSprinting && !sprint)
			{
				FOVModifier = 10;
				
				Network.EntityAction((int) EntityId, EntityAction.StartSprinting);
			}
			else if (!IsSprinting && sprint)
			{
				FOVModifier = 0;
				
				Network.EntityAction((int)EntityId, EntityAction.StopSprinting);
			}

			if (IsSneaking != sneak)
			{
				if (IsSneaking)
				{
					Network.EntityAction((int)EntityId, EntityAction.StartSneaking);	
					Camera.UpdateOffset(new Vector3(0f, -0.15f, 0.35f));
				}
				else
				{
					Network.EntityAction((int)EntityId, EntityAction.StopSneaking);
					Camera.UpdateOffset(Vector3.Zero);
				}
			}
			
		//	DoHealthAndExhaustion();

			var previousCheckedInput = _prevCheckedInput;
			
			if ((Controller.CheckInput && Controller.CheckMovementInput))
			{
				_prevCheckedInput = true;
				if (!previousCheckedInput || World.FormManager.IsShowingForm)
				{
					return;
				}
				
				UpdateRayTracer();

				//if (Controller.InputManager.IsDown(InputCommand.LeftClick) && DateTime.UtcNow - _lastAnimate >= TimeSpan.FromMilliseconds(500))
				//{
				//	SwingArm(true);
				//}
				
				var hitEntity = HitEntity;
				if (hitEntity != null && Controller.InputManager.IsPressed(InputCommand.LeftClick) && hitEntity is LivingEntity)
				{
					if (_destroyingBlock)
						StopBreakingBlock(forceCanceled:true);
					
					InteractWithEntity(hitEntity, true);
				}
				else if (hitEntity != null && Controller.InputManager.IsPressed(InputCommand.RightClick) && hitEntity is LivingEntity)
				{
					if (_destroyingBlock)
						StopBreakingBlock(forceCanceled:true);
					
					InteractWithEntity(hitEntity, false);
				}
				else if (hitEntity == null && !_destroyingBlock
				                           && Controller.InputManager.IsPressed(InputCommand.LeftClick)
				                           && !HasRaytraceResult)
				{
					HandleLeftClick(Inventory[Inventory.SelectedSlot], Inventory.SelectedSlot);
				}
				else if (hitEntity == null && !_destroyingBlock && Controller.InputManager.IsDown(InputCommand.LeftClick) && !IsWorldImmutable && HasRaytraceResult) //Destroying block.
				{
					StartBreakingBlock();
				}
				else if (_destroyingBlock && Controller.InputManager.IsUp(InputCommand.LeftClick))
				{
					StopBreakingBlock();
				}
				else if (_destroyingBlock && Controller.InputManager.IsDown(InputCommand.LeftClick))
				{
					if (_destroyingTarget != new BlockCoordinates(Vector3.Floor(Raytraced)))
					{
						StopBreakingBlock(true, true);

						if (Gamemode != Gamemode.Creative)
						{
							StartBreakingBlock();
						}
					}
					else
					{
						if ((DateTime.UtcNow - _lastAnimate).TotalMilliseconds > 500)
						{
							_lastAnimate = DateTime.UtcNow;
							SwingArm(true);
						}
						
						var timeRan = (DateTime.UtcNow - _destroyingTick).TotalMilliseconds / 50d;
						if (timeRan >= _destroyTimeNeeded)
						{
							StopBreakingBlock(true);
						}
					}
				}
				else if (Controller.InputManager.IsPressed(InputCommand.RightClick))
				{
					bool handledClick = false;
					var item = Inventory[Inventory.SelectedSlot];
					// Log.Debug($"Right click!");
					if (item != null)
					{
						handledClick = HandleClick(item, Inventory.SelectedSlot);
					}

					/*if (!handledClick && Inventory.OffHand != null && !(Inventory.OffHand is ItemAir))
					{
						handledClick = HandleRightClick(Inventory.OffHand, 1);
					}*/
				}

				if (hitEntity != null && HasCollision)
				{
					if (IsColliding(hitEntity))
					{
						//var distance = DistanceToHorizontal(hitEntity);
					//	Velocity += (KnownPosition.ToVector3() - hitEntity.KnownPosition.ToVector3());
					}
				}
            }
			else
			{
				if (_destroyingBlock)
				{
					StopBreakingBlock();
				}

				_prevCheckedInput = false;
				_lastTimeWithoutInput = DateTime.UtcNow;
			}

			if (PreviousSlot != Inventory.SelectedSlot)
			{
				var slot = Inventory.SelectedSlot;
				Network?.HeldItemChanged(Inventory[Inventory.SelectedSlot], (short) slot);
				PreviousSlot = slot;
			}

			base.Update(args);

		}

	    public void Jump()
	    {
		    HealthManager.Exhaust(IsSprinting ? 0.2f : 0.05f);
		    
		    Velocity += new Vector3(0f, MathF.Sqrt(2f * (float) Gravity * 1.2f), 0f);
		    Network?.EntityAction((int) EntityId, EntityAction.Jump);
	    }

	    private void InteractWithEntity(Entity entity, bool attack)
	    {
		    SwingArm(true);
		    
		    bool canAttack = true;

		    if (entity is PlayerMob)
		    {
			    canAttack = !IsNoPvP && Level.Pvp;
		    }
		    else
		    {
			    canAttack = !IsNoPvM;
		    }

		  //  Log.Info($"Interacting with entity. Attack: {attack} - CanAttack: {canAttack} - PVM: {IsNoPvM} - PVP: {IsNoPvP}");
		    
		    if (attack)
		    {
			   // entity.EntityHurt();
			    Network?.EntityInteraction(this, entity, McpeInventoryTransaction.ItemUseOnEntityAction.Attack);
		    }
		    else
		    {
			    Network?.EntityInteraction(this, entity, McpeInventoryTransaction.ItemUseOnEntityAction.Interact);
		    }
	    }

	    public Entity HitEntity { get; private set; } = null;
	    public Entity[] EntitiesInRange { get; private set; } = null;

	    private void UpdateRayTracer()
	    {
		    var camPos = Level.Camera.Position;
		    var lookVector = Level.Camera.Direction;

		    var entities = Level.EntityManager.GetEntities(camPos, 8);
		    EntitiesInRange = entities.ToArray();

		    if (EntitiesInRange.Length == 0)
		    {
			    HitEntity = null;
			    return;
		    }
		    
		    Entity hitEntity = null;
		    for (float x = 0.5f; x < 8f; x += 0.1f)
		    {
			    Vector3 targetPoint = camPos + (lookVector * x);
			    var entity = EntitiesInRange.FirstOrDefault(xx =>
				    xx.GetBoundingBox().Contains(targetPoint) == ContainmentType.Contains);

			    if (entity != null)
			    {
				    hitEntity = entity;
				    break;
			    }
		    }

		    HitEntity = hitEntity;
	    }

	    private void BlockBreakTick()
	    {
		    //_destroyingTick++;
        }

	    private void StartBreakingBlock()
	    {
		    SwingArm(true);
		    
			var floored =  Vector3.Floor(Raytraced);

		    var block = Level.GetBlock(floored);
		    if (!block.HasHitbox)
		    {
			    return;
		    }

            _destroyingBlock = true;
		    _destroyingTarget = floored;
		    _destroyingFace = GetTargetFace();
		    _destroyingTick = DateTime.UtcNow;

		    //if (Inventory.MainHand != null)
		    {
			    _destroyTimeNeeded = block.GetBreakTime(Inventory.MainHand ?? new ItemAir()) * 20f;
		    }

            Log.Debug($"Start break block ({_destroyingTarget}, {_destroyTimeNeeded} ticks.)");

            var flooredAdj = Vector3.Floor(AdjacentRaytrace);
            var remainder = new Vector3(AdjacentRaytrace.X - flooredAdj.X, AdjacentRaytrace.Y - flooredAdj.Y, AdjacentRaytrace.Z - flooredAdj.Z);

            Network?.PlayerDigging(DiggingStatus.Started, _destroyingTarget, _destroyingFace, remainder);
        }

	    private void StopBreakingBlock(bool sendToServer = true, bool forceCanceled = false)
	    {
		    var end = DateTime.UtcNow;
		    _destroyingBlock = false;
           // var ticks = Interlocked.Exchange(ref _destroyingTick, 0);// = 0;
		    var start = _destroyingTick;
			_destroyingTick = DateTime.MaxValue;

		    var timeRan = (end - start).TotalMilliseconds / 50d;

            var flooredAdj = Vector3.Floor(AdjacentRaytrace);
            var remainder = new Vector3(AdjacentRaytrace.X - flooredAdj.X, AdjacentRaytrace.Y - flooredAdj.Y, AdjacentRaytrace.Z - flooredAdj.Z);

            if (!sendToServer)
		    {
			    Log.Debug($"Stopped breaking block, not notifying server. Time: {timeRan}");
                return;
		    }

		    if ((Gamemode == Gamemode.Creative  || timeRan >= _destroyTimeNeeded) && !forceCanceled)
		    {
                Network?.PlayerDigging(DiggingStatus.Finished, _destroyingTarget, _destroyingFace, remainder);
			    Log.Debug($"Stopped breaking block. Ticks passed: {timeRan}");

				Level.SetBlockState(_destroyingTarget, new Air().GetDefaultState());
            }
		    else
		    {
			    Network?.PlayerDigging(DiggingStatus.Cancelled, _destroyingTarget, _destroyingFace, remainder);
			    Log.Debug($"Cancelled breaking block. Tick passed: {timeRan}");
            }
	    }

	    private BlockFace GetTargetFace()
	    {
		    var flooredAdj =  Vector3.Floor(AdjacentRaytrace);
		    var raytraceFloored  = Vector3.Floor(Raytraced);

		    var adj = flooredAdj - raytraceFloored;
		    adj.Normalize();

		    return adj.GetBlockFace();
        }

	    private void HandleLeftClick(Item slot, int hand)
	    {
		    HandleClick(slot, hand, false, true);
	    }

	    private bool HandleClick(Item slot, int hand, bool canModifyWorld = true, bool isLeftClick = false)
	    {
		    SwingArm(true);
		    //if (ItemFactory.ResolveItemName(slot.ItemID, out string itemName))
		    {
			    var flooredAdj = Vector3.Floor(AdjacentRaytrace);
			    var raytraceFloored = Vector3.Floor(Raytraced);

			    var adj = flooredAdj - raytraceFloored;
			    adj.Normalize();

			    var face = adj.GetBlockFace();

			    var remainder = new Vector3(AdjacentRaytrace.X - flooredAdj.X,
				    AdjacentRaytrace.Y - flooredAdj.Y, AdjacentRaytrace.Z - flooredAdj.Z);

			    var coordR = new BlockCoordinates(raytraceFloored);
			    
			    //IBlock block = null;
			    if (!IsWorldImmutable && HasRaytraceResult)
			    {
				    var existingBlock = Level.GetBlock(coordR);
				    bool isBlockItem = slot is ItemBlock;
				    
				    if (existingBlock.CanInteract && (!isBlockItem || IsSneaking))
				    {
					    Network?.WorldInteraction(coordR, face, hand, remainder);

					    return true;
				    }
				    
				    if (slot is ItemBlock ib && canModifyWorld)
				    {
					    BlockState blockState = ib.Block;

					    if (blockState != null && !(blockState.Block is Air) && HasRaytraceResult)
					    {
						    if (existingBlock.IsReplacible || !existingBlock.Solid)
						    {
							    if (CanPlaceBlock(coordR, (Block) blockState.Block))
							    {
								    Level.SetBlockState(coordR, blockState);

								    Network?.BlockPlaced(coordR.BlockDown(), BlockFace.Up, hand, remainder, this);

								    return true;
							    }
						    }
						    else
						    {
							    var target = new BlockCoordinates(raytraceFloored + adj);
							    if (CanPlaceBlock(target, (Block) blockState.Block))
							    {
								    Level.SetBlockState(target, blockState);

								    Network?.BlockPlaced(coordR, face, hand, remainder, this);
								    
								    return true;
							    }
						    }
					    }
				    }
			    }

			    if (!(slot is ItemAir) && slot.Id > 0 && slot.Count > 0)
			    {
				    ItemUseAction action;
	                if (isLeftClick)
	                {
		                action = HasRaytraceResult ? ItemUseAction.ClickBlock : ItemUseAction.ClickAir;
	                }
	                else
	                {
		                action = HasRaytraceResult ? ItemUseAction.RightClickBlock : ItemUseAction.RightClickAir;
	                }
	                
                    Network?.UseItem(slot, hand, action);
                    return true;
                }
            }

		    return false;
	    }

	    private bool CanPlaceBlock(BlockCoordinates coordinates, Block block)
	    {
		    var bb = block.GetBoundingBox(coordinates);
		    var playerBb = GetBoundingBox(KnownPosition);

		    if (playerBb.Intersects(bb))
		    {
			    return false;
		    }

		    return true;
	    }

	    public override BoundingBox GetBoundingBox(Vector3 pos)
		{
			double halfWidth = (0.6 * Scale) / 2D;
			var height = IsSneaking ? 1.5 : Height;
			
			return new BoundingBox(
				new Vector3((float) (pos.X - halfWidth), pos.Y, (float) (pos.Z - halfWidth)),
				new Vector3(
					(float) (pos.X + halfWidth), (float) (pos.Y + (height * Scale)), (float) (pos.Z + halfWidth)));
		}

		public override void OnTick()
		{
			if (_destroyingBlock)
			{
				BlockBreakTick();
			}

			base.OnTick();
		}
	}
}