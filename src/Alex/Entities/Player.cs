using System.Linq;
using System.Threading;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Input;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Items;
using Alex.Net;
using Alex.Utils.Inventories;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using ContainmentType = Microsoft.Xna.Framework.ContainmentType;
using Skin = Alex.API.Utils.Skin;

namespace Alex.Entities
{
    public class Player : RemotePlayer
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Player));

        public static readonly float EyeLevel = 1.625F;
        public static readonly float Height = 1.8F;

		public PlayerIndex PlayerIndex { get; }

		public PlayerController Controller { get; }
        public Vector3 Raytraced = Vector3.Zero;
        public Vector3 AdjacentRaytrace = Vector3.Zero;

        public bool HasRaytraceResult = false;

        public bool IsWorldImmutable { get; set; } = false;
        public bool IsNoPvP { get; set; } = true;
        public bool IsNoPvM { get; set; } = true;
        
        //public Camera Camera { get; internal set; }
        public Player(GraphicsDevice graphics, InputManager inputManager, string name, World world, Skin skin, NetworkProvider networkProvider, PlayerIndex playerIndex) : base(name, world, networkProvider, skin.Texture)
        {
	        IsSpawned = true;
		//	DoRotationCalculations = false;
			PlayerIndex = playerIndex;
		    Controller = new PlayerController(graphics, world, inputManager, this, playerIndex);
		   // Camera = camera;
		   // NoAi = false;

			//Inventory = new Inventory(46);
			//Inventory.SelectedHotbarSlotChanged += SelectedHotbarSlotChanged;
			//base.Inventory.IsPeInventory = true;
			//MovementSpeed = 0.1f;
		//	BaseMovementSpeed = 0.21585;//4.317;
		//	FlyingSpeed = 0.5f; //10.89f;
			
			SnapHeadYawRotationOnMovement = false;
			SnapYawRotationOnMovement = true;
			DoRotationCalculations = false;
			
			RenderEntity = true;
			ShowItemInHand = true;

			ServerEntity = false;
			RequiresRealTimeTick = true;
			AlwaysTick = true;
        }

        /// <inheritdoc />
        public override bool NoAi { get; set; } = false;

        protected override void OnInventorySlotChanged(object sender, SlotChangedEventArgs e)
        {
	        //Crafting!
	    /*    if (e.Index >= 41 && e.Index <= 44)
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
	        }*/
	        
	        base.OnInventorySlotChanged(sender, e);
        }

        /// <inheritdoc />
        public override void CollidedWithWorld(Vector3 direction, Vector3 position, float impactVelocity)
        {
	        //var dirVelocity = direction * impactVelocity;
	        if (direction == Vector3.Down)
	        {
		        //Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
		        KnownPosition.OnGround = true;
		        StopFalling();
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

        public bool IsBreakingBlock => _destroyingBlock;

	    public float BlockBreakProgress
	    {
		    get
		    {
			    if (!IsBreakingBlock)
				    return 0;

			    return (float) ((1f / (float) _destroyTimeNeeded) * _destroyingTick);
		    }
	    }

	    public double BreakTimeNeeded
	    {
		    set
		    {
			    _destroyTimeNeeded = value;
		    }
	    }

	    private bool _waitingOnChunk = true;

	    public bool WaitingOnChunk
	    {
		    get
		    {
			    return _waitingOnChunk;
		    }
		    set
		    {
			    _waitingOnChunk = value;

			    if (value)
			    {
				    NoAi = true;
			    }
			    else
			    {
				    Velocity = Vector3.Zero;
				    NoAi = false;
			    }
		    }
	    }
	    
	    public BlockCoordinates TargetBlock => _destroyingTarget;

	    private BlockCoordinates _destroyingTarget = BlockCoordinates.Zero;
	    private bool _destroyingBlock = false;
        private int _destroyingTick = 0;
	    private double _destroyTimeNeeded = 0;
	    private API.Blocks.BlockFace _destroyingFace;

	    private int  PreviousSlot { get; set; } = -1;
	    public  bool CanSprint    => HealthManager.Hunger > 6;
	    private bool _skipUpdate = false;

	    internal void SkipUpdate()
	    {
		    _skipUpdate = true;
	    }
	    
	    public override void Update(IUpdateArgs args)
	    {
		    if (WaitingOnChunk && Age % 4 == 0)
		    {
			    NoAi = true;

			    if (Level.GetChunk(KnownPosition.GetCoordinates3D(), true) != null)
			    {
				    WaitingOnChunk = false;
			    }
		    }

		    bool sprint = IsSprinting;
		    bool sneak  = IsSneaking;

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
			    Network.EntityAction((int) EntityId, EntityAction.StopSprinting);
		    }

		    if (IsSneaking != sneak)
		    {
			    if (IsSneaking)
			    {
				    Network.EntityAction((int) EntityId, EntityAction.StartSneaking);
				    Level.Camera.UpdateOffset(new Vector3(0f, -0.15f, 0.35f));
			    }
			    else
			    {
				    Network.EntityAction((int) EntityId, EntityAction.StopSneaking);
				    Level.Camera.UpdateOffset(Vector3.Zero);
			    }
		    }

		    //	DoHealthAndExhaustion();

		    //var previousCheckedInput = _prevCheckedInput;

		    if (_skipUpdate)
		    {
			    _skipUpdate = false;
		    }
		    else if ((Controller.CheckInput && Controller.CheckMovementInput && Alex.Instance.GuiManager.ActiveDialog == null))
		    {

			    UpdateBlockRayTracer();
			    UpdateRayTracer();

			    //if (Controller.InputManager.IsDown(InputCommand.LeftClick) && DateTime.UtcNow - _lastAnimate >= TimeSpan.FromMilliseconds(500))
			    //{
			    //	SwingArm(true);
			    //}

			    bool didLeftClick     = Controller.InputManager.IsPressed(InputCommand.LeftClick);
			    bool didRightClick    = Controller.InputManager.IsPressed(InputCommand.RightClick);
			    bool leftMouseBtnDown = Controller.InputManager.IsDown(InputCommand.LeftClick);

			    var hitEntity = HitEntity;

			    if (hitEntity != null && didLeftClick && hitEntity is LivingEntity)
			    {
				    if (_destroyingBlock)
					    StopBreakingBlock(forceCanceled: true);

				    InteractWithEntity(hitEntity, true, IsLeftHanded ? 1 : 0);
			    }
			    else if (hitEntity != null && didRightClick && hitEntity is LivingEntity)
			    {
				    if (_destroyingBlock)
					    StopBreakingBlock(forceCanceled: true);

				    InteractWithEntity(hitEntity, false, IsLeftHanded ? 1 : 0);
			    }
			    else if (hitEntity == null && !_destroyingBlock && didLeftClick && !HasRaytraceResult)
			    {
				    HandleLeftClick(IsLeftHanded ? Inventory.OffHand : Inventory.MainHand, IsLeftHanded ? 1 : 0);
			    }
			    else if (hitEntity == null && !_destroyingBlock
			                               && Controller.InputManager.IsBeginPress(InputCommand.LeftClick)
			                               && !IsWorldImmutable && HasRaytraceResult) //Destroying block.
			    {
				    StartBreakingBlock();
			    }
			    else if (_destroyingBlock)
			    {
				    if (!leftMouseBtnDown)
				    {
					    StopBreakingBlock();
				    }
				    else if (_destroyingTarget != new BlockCoordinates(Vector3.Floor(Raytraced)))
				    {
					    StopBreakingBlock(true, true);

					    if (Gamemode != Gamemode.Creative)
					    {
						    //	StartBreakingBlock();
					    }
				    }
			    }
			    else if (didRightClick)
			    {
				    bool handledClick = false;
				    var  item = IsLeftHanded ? Inventory.OffHand : Inventory.MainHand; // [Inventory.SelectedSlot];

				    // Log.Debug($"Right click!");
				    if (item != null)
				    {
					    handledClick = HandleClick(
						    item, IsLeftHanded ? 1 : 0, Inventory.HotbarOffset + Inventory.SelectedSlot);
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
		    }

		    if (PreviousSlot != Inventory.SelectedSlot)
		    {
			    var slot = Inventory.SelectedSlot;
			    Network?.HeldItemChanged(Inventory[Inventory.SelectedSlot], (short) slot);
			    PreviousSlot = slot;
		    }

		    base.Update(args);

	    }

	    private void InteractWithEntity(Entity entity, bool attack, int hand)
	    {
		    Log.Info($"Entity interact detected. Attack: {attack}");
		    SwingArm(true);
		    
		    bool canAttack = true;

		    if (entity is RemotePlayer)
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
			    Network?.EntityInteraction(this, entity, ItemUseOnEntityAction.Attack, hand, hand == 1 ? Inventory.OffHandSlot : Inventory.SelectedSlot);
		    }
		    else
		    {
			    Network?.EntityInteraction(this, entity, ItemUseOnEntityAction.Interact, hand, hand == 1 ? Inventory.OffHandSlot : Inventory.SelectedSlot);
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

	    public Block   SelBlock              { get; private set; } = null;
	    public Vector3 RaytracedBlock        { get; private set; }
	    public Vector3 AdjacentRaytraceBlock { get; private set; }
	    private void UpdateBlockRayTracer()
	    {
		    var camPos     = Level.Camera.Position;
		    var lookVector = Level.Camera.Direction;

		    for (float x = 0.5f; x < 8f; x += 0.1f)
		    {
			    Vector3 targetPoint  = camPos + (lookVector * x);
			    
			    var     flooredBlock = Vector3.Floor(targetPoint);
			    var     block        = Level.GetBlockState(targetPoint);

			    if (block != null && block.Block.HasHitbox)
			    {
				    foreach (var bbox in block.Model.GetBoundingBoxes(flooredBlock))
				    {
					    if (bbox.Contains(targetPoint) == ContainmentType.Contains)
					    {
						    RaytracedBlock = Vector3.Floor(targetPoint);
						    SelBlock = block.Block;
						    //  RayTraceBoundingBox = bbox;

						    Raytraced = targetPoint;
						    HasRaytraceResult = true;

						    if (SetPlayerAdjacentSelectedBlock(Level, x, camPos, lookVector, out Vector3 rawAdjacent))
						    {
							    AdjacentRaytraceBlock = Vector3.Floor(rawAdjacent);

							    AdjacentRaytrace = rawAdjacent;
						    }

						    return;
					    }
				    }
			    }
		    }

		    SelBlock = null;
		    HasRaytraceResult = false;
	    }
	    
	    private bool SetPlayerAdjacentSelectedBlock(World world, float xStart, Vector3 camPos, Vector3 lookVector, out Vector3 rawAdjacent)
	    {
		    for (float x = xStart; x > 0.7f; x -= 0.1f)
		    {
			    Vector3 targetPoint = camPos + (lookVector * x);
			    var     block       = world.GetBlock(targetPoint) as Block;

			    if (block != null && (!block.Solid))
			    {
				    rawAdjacent = targetPoint;
				    return true;
			    }
		    }

		    rawAdjacent = new Vector3(0, 0, 0);
		    return false;
	    }

	    private void BlockBreakTick()
	    {
		    var tick =  Interlocked.Increment(ref _destroyingTick);
		    if (tick % 10 == 0)
		    {
			    SwingArm(true);
		    }
		    
		    if (tick >= _destroyTimeNeeded)
		    {
			    StopBreakingBlock(true);
		    }
	    }

	    private void StartBreakingBlock()
	    {
		    SwingArm(true);
		    
			var floored  = new BlockCoordinates(Vector3.Floor(Raytraced));
			var adjacent = AdjacentRaytrace;
			
		    var block    = Level.GetBlock(floored);
		    if (!block.HasHitbox)
		    {
			    return;
		    }

		    var face = GetTargetFace();

		    _destroyingBlock = true;
		    _destroyingTarget = floored;
		    _destroyingFace = face;
		    
		    Interlocked.Exchange(ref _destroyingTick, 0);
		    
		    _destroyTimeNeeded = block.GetBreakTime(Inventory.MainHand ?? new ItemAir());

		    Log.Debug($"Start break block ({_destroyingTarget}, {_destroyTimeNeeded} ticks.)");

            var flooredAdj = Vector3.Floor(adjacent);
            var remainder = new Vector3(adjacent.X - flooredAdj.X, adjacent.Y - flooredAdj.Y, adjacent.Z - flooredAdj.Z);

            Network?.PlayerDigging(DiggingStatus.Started, floored, face, remainder);
        }

	    private void StopBreakingBlock(bool sendToServer = true, bool forceCanceled = false)
	    {
		    _destroyingBlock = false;
		    
            var ticks = Interlocked.Exchange(ref _destroyingTick, 0);// = 0;

            var flooredAdj = Vector3.Floor(AdjacentRaytrace);
            var remainder = new Vector3(AdjacentRaytrace.X - flooredAdj.X, AdjacentRaytrace.Y - flooredAdj.Y, AdjacentRaytrace.Z - flooredAdj.Z);

            if (!sendToServer)
		    {
			    Log.Debug($"Stopped breaking block, not notifying server. Time: {ticks}");
                return;
		    }

		    if ((Gamemode == Gamemode.Creative  || ticks >= _destroyTimeNeeded) && !forceCanceled)
		    {
                Network?.PlayerDigging(DiggingStatus.Finished, _destroyingTarget, _destroyingFace, remainder);
			    Log.Debug($"Stopped breaking block. Ticks passed: {ticks}");

				Level.SetBlockState(_destroyingTarget, new Air().GetDefaultState());
            }
		    else
		    {
			    Network?.PlayerDigging(DiggingStatus.Cancelled, _destroyingTarget, _destroyingFace, remainder);
			    Log.Debug($"Cancelled breaking block. Tick passed: {ticks}");
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
		    HandleClick(slot, hand, Inventory.HotbarOffset + Inventory.SelectedSlot, false, true);
	    }

	    private bool HandleClick(Item slot, int hand, int inventorySlot, bool canModifyWorld = true, bool isLeftClick = false)
	    {
		  //  Log.Info($"Clicky clicky click. Left click: {isLeftClick} Can modify world: {canModifyWorld} HasRaytrace: {HasRaytraceResult}");
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
			    if (/*!IsWorldImmutable &&*/ HasRaytraceResult)
			    {
				    var existingBlock = Level.GetBlock(coordR);
				    bool isBlockItem = slot is ItemBlock;
				    
				    if (existingBlock.CanInteract && (!isBlockItem || IsSneaking))
				    {
					    Network?.WorldInteraction(this, coordR, face, hand, inventorySlot, remainder);
						Log.Info($"World interaction.");
					    return true;
				    }
				    
				    if (slot is ItemBlock ib && canModifyWorld)
				    {
					   // Log.Info($"Placing block.");
					    BlockState blockState = ib.Block;

					    if (blockState != null && !(blockState.Block is Air) && HasRaytraceResult)
					    {
						    if (existingBlock.IsReplacible || !existingBlock.Solid)
						    {
							//    Log.Info($"Placing block 1");
							    if (CanPlaceBlock(coordR, (Block) blockState.Block))
							    {
								    Level.SetBlockState(coordR, blockState);

								    Network?.BlockPlaced(coordR.BlockDown(), BlockFace.Up, hand, inventorySlot, remainder, this);

								    return true;
							    }
						    }
						    else
						    {
							//    Log.Info($"Placing block 2");
							    var target = new BlockCoordinates(raytraceFloored + adj);
							    if (CanPlaceBlock(target, (Block) blockState.Block))
							    {
								    Level.SetBlockState(target, blockState);

								    Network?.BlockPlaced(coordR, face, hand, inventorySlot, remainder, this);
								    
								    return true;
							    }
						    }
					    }
				    }
				    else if (!(slot is ItemBlock))
				    {
					   // Log.Info($"Item is not a block, got type of: {slot.GetType()}");
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
	                
                    Network?.UseItem(slot, hand, action, coordR, face, remainder);
                    return true;
                }
            }

		    return false;
	    }

	    private bool CanPlaceBlock(BlockCoordinates coordinates, Block block)
	    {
		    var bb = block.BlockState.Model.GetBoundingBox(coordinates);
		    var playerBb = BoundingBox;

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

	    private bool  Falling      { get; set; } = false;
	    private float FallingStart { get; set; } = 0;

	    private void StopFalling()
	    {
		    if (!Falling)
			    return;
		    
		    float fallStart = FallingStart;
		    float y         = KnownPosition.Y;
		    Falling = false;

		    if (fallStart > y)
			    return;

		    float distance = fallStart - y;
		    bool  inVoid   = y < 0;
		    
			Network?.EntityFell(EntityId, distance, inVoid);
			
			Network?.PlayerOnGroundChanged(this, true);
	    }

	    private void StartFalling()
	    {
		    Falling = true;
		    FallingStart = KnownPosition.Y;

		    Network?.PlayerOnGroundChanged(this, false);
	    }
	    
	    private float _fovModifier      = 0f;

	    public float FOVModifier
	    {
		    get => _fovModifier;
		    set
		    {
			    _fovModifier = value;
			    Level.Camera.FOVModifier = value;
		    }
	    }

	    //private vector
	    public override void OnTick()
		{
			if (_destroyingBlock)
			{
				BlockBreakTick();
			}

			if (!IsFlying)
			{
				if (!Falling && !KnownPosition.OnGround)
				{
					StartFalling();
				}
				else if (Falling &&( KnownPosition.Y <= -40 || KnownPosition.OnGround))
				{
					StopFalling();
				}
			}
			
			base.OnTick();
		}
	}
}