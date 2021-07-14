using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Graphics;
using Alex.Common.Input;
using Alex.Common.Items;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Items;
using Alex.Net;
using Alex.Net.Bedrock;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Utils.Inventories;
using Alex.Utils.Skins;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Bedrock;
using LibNoise.Combiner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.LevelDB;
using MiNET.Net;
using MiNET.Utils.Skins;
using MiNET.Worlds;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Fluent;
using RocketUI.Input;
using Biome = Alex.Worlds.Biome;
using BlockCoordinates = Alex.Common.Utils.Vectors.BlockCoordinates;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using ContainmentType = Microsoft.Xna.Framework.ContainmentType;
using Skin = Alex.Common.Utils.Skin;
using SkinResourcePatch = Alex.Utils.Skins.SkinResourcePatch;

namespace Alex.Entities
{
    public class Player : RemotePlayer
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Player));

        public static readonly float EyeLevel = 1.625F;
        public static readonly float Height = 1.8F;

		//public PlayerIndex PlayerIndex { get; }

		public PlayerController Controller { get; }
		private Vector3 _raytraced = Vector3.Zero;
		private Vector3 _adjacentRaytrace = Vector3.Zero;

        public bool HasRaytraceResult = false;

        public NetworkProvider Network { get; set; }

        /// <inheritdoc />
        public override PlayerLocation KnownPosition
        {
	        get
	        {
		        return base.KnownPosition;
	        }
	        set
	        {
		        base.KnownPosition = value;

		        if (Level == null)
			        return;
		        
		        if (!Level.ChunkManager.TryGetChunk(new ChunkCoordinates(value), out _))
		        {
			        WaitingOnChunk = true;
			        // NoAi = false;
		        }
	        }
        }

        //public Camera Camera { get; internal set; }
        public Player(GraphicsDevice graphics, InputManager inputManager, World world, NetworkProvider networkProvider, PlayerIndex playerIndex) : base(world)
        {
	        Network = networkProvider;
	        
		    Controller = new PlayerController(graphics, world, inputManager, this, playerIndex);

		    SnapHeadYawRotationOnMovement = false;
			SnapYawRotationOnMovement = true;
			DoRotationCalculations = false;
			
			RenderEntity = true;
			//ShowItemInHand = true;

		//	ServerEntity = false;
	//		AlwaysTick = true;
			
			IsAffectedByGravity = true;
			HasPhysics = true;
			NoAi = false;
			CanSwim = true;
        }

        /// <inheritdoc />
        public override float CollidedWithWorld(Vector3 direction, Vector3 position, float impactVelocity)
        {
	        //var dirVelocity = direction * impactVelocity;
	        if (direction == Vector3.Down)
	        {
		        //Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
		        KnownPosition.OnGround = true;
		        StopFalling();
		        
		       // if (bs?.Block is SlimeBlock sb)
		       // {
			       // if (MathF.Abs(imp))
			    //    return -(impactVelocity * 0.8f);
		        //}
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

        public bool IsBreakingBlock => _destroyingBlock;

	    public float BlockBreakProgress
	    {
		    get
		    {
			    if (!IsBreakingBlock)
				    return 0;

			    return (1f / (float) _destroyTimeNeeded) * _destroyingTick;
		    }
	    }

	    public double BreakTimeNeeded
	    {
		    set
		    {
			    _destroyTimeNeeded = value;
		    }
	    }

	    public bool WaitingOnChunk { get; set; } = true;

	    /// <inheritdoc />
	    public override bool NoAi 
	    { 
		    get => base.NoAi || WaitingOnChunk;
		    set => base.NoAi = value;
	    }

	    public BlockCoordinates TargetBlock => _destroyingTarget;

	    private BlockCoordinates _destroyingTarget = BlockCoordinates.Zero;
	    private bool _destroyingBlock = false;
        private int _destroyingTick = 0;
	    private double _destroyTimeNeeded = 0;
	    private BlockFace _destroyingFace;

	    private int  PreviousSlot { get; set; } = -1;
	    public  bool CanSprint    => HealthManager.Hunger > 6;
	    private bool _skipUpdate = false;

	    internal void SkipUpdate()
	    {
		    _skipUpdate = true;
	    }

	    private bool _previousHasActiveDialog = false;
	    public Biome CurrentBiome { get; private set; }
	    public override void Update(IUpdateArgs args)
	    {
		    base.Update(args);
		    
		    bool hasActiveDialog = Alex.Instance.GuiManager.ActiveDialog != null || ((Network is BedrockClient c) && c.WorldProvider.FormManager.IsShowingForm);
		    Controller.CheckMovementInput = !hasActiveDialog;
		    
		    if (WaitingOnChunk)
		    {
			  //  NoAi = true;

			    if (Level.GetChunk(KnownPosition.GetCoordinates3D(), true) != null)
			    {
				    WaitingOnChunk = false;
				   // NoAi = false;
			    }
		    }

		    if (!IsSpawned)
		    {
			    base.Update(args);
			    return;
		    }

		    CurrentBiome = Level.GetBiome(RenderLocation.GetCoordinates3D());
		    
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
		    else if ((Controller.CheckInput && Controller.CheckMovementInput && !hasActiveDialog && !_previousHasActiveDialog))
		    {

			    UpdateBlockRayTracer();
			    UpdateRayTracer();

			    //if (Controller.InputManager.IsDown(InputCommand.LeftClick) && DateTime.UtcNow - _lastAnimate >= TimeSpan.FromMilliseconds(500))
			    //{
			    //	SwingArm(true);
			    //}

			    bool didLeftClick     = Controller.InputManager.IsPressed(AlexInputCommand.LeftClick);
			    bool didRightClick    = Controller.InputManager.IsPressed(AlexInputCommand.RightClick);
			    bool leftMouseBtnDown = Controller.InputManager.IsDown(AlexInputCommand.LeftClick);
			    bool rightMouseBtnDown = Controller.InputManager.IsDown(AlexInputCommand.RightClick);
			    bool beginLeftClick = Controller.InputManager.IsBeginPress(InputCommand.LeftClick);
			    bool beginRightClick = Controller.InputManager.IsBeginPress(InputCommand.RightClick);
			    
			    if (IsUsingItem)
			    {
				    if (!leftMouseBtnDown && !rightMouseBtnDown)
				    {
					    StopUseItem();
				    }
			    }
			    else
			    {
				    if (beginLeftClick || beginRightClick)
				    {
					    BeginUseItem(beginLeftClick);
				    }
			    }

			    var hitEntity = HitEntity;

			    if (hitEntity != null)
			    {
				    if (hitEntity is LivingEntity)
				    {
					    if (didLeftClick || didRightClick)
					    {
						    if (_destroyingBlock)
							    StopBreakingBlock(forceCanceled: true);

						    var adjacent = EntityTracerPoint;
						    var flooredAdj = Vector3.Floor(adjacent);
						    var remainder = new Vector3(adjacent.X - flooredAdj.X, adjacent.Y - flooredAdj.Y, adjacent.Z - flooredAdj.Z);
						    
						    InteractWithEntity(hitEntity, didLeftClick, IsLeftHanded ? 1 : 0, remainder);
						    return;
					    }
				    }
			    }
			    else
			    {
				    if (_destroyingBlock)
				    {
					    if (!leftMouseBtnDown)
					    {
						    StopBreakingBlock();
					    }
					    else if (_destroyingTarget != new BlockCoordinates(Vector3.Floor(_raytraced)))
					    {
						    StopBreakingBlock(true);

						    if (Gamemode != GameMode.Creative)
						    {
							    //	StartBreakingBlock();
						    }
					    }
				    }
				    else
				    {
					    if (HasRaytraceResult)
					    {
						    if (beginLeftClick && !IsWorldImmutable)
						    {
							    StartBreakingBlock();
						    }
					    }
					    else
					    {
						    if (didLeftClick)
						    {
							    HandleLeftClick(IsLeftHanded ? Inventory.OffHand : Inventory.MainHand, IsLeftHanded ? 1 : 0);
						    }
					    }
				    }
				   
			    }
			    
			    if (didRightClick)
			    {
				    bool handledClick = false;
				    var  item = IsLeftHanded ? Inventory.OffHand : Inventory.MainHand; // [Inventory.SelectedSlot];
				    
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
		    }
		    else
		    {
			    IsUsingItem = false;
			    
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

		    _previousHasActiveDialog = hasActiveDialog;
		    if (hasActiveDialog)
			    _skipUpdate = true;

		    //if (FeetInWater && HeadInWater)
			//    IsSwimming = true;
		    //else
			//    IsSwimming = false;
	    }

	    private void InteractWithEntity(Entity entity, bool attack, int hand, Vector3 cursorPosition)
	    {
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

		    if (IsSneaking)
		    {
			    /*if (rp.Skin != null)
			    {
				    if (!Directory.Exists("skins"))
					    Directory.CreateDirectory("skins");

				    var skinPath = Path.Combine("skins", $"{rp.GeometryName}.json");
				    var skinTexturePath = Path.Combine("skins", $"{rp.GeometryName}.png");
				    File.WriteAllText(skinPath, rp.Skin.GeometryData);

				    var texture = rp.ModelRenderer.Texture;

				    using (FileStream fs = File.OpenWrite(skinTexturePath))
				    {
					    texture.SaveAsPng(fs, texture.Width, texture.Height);
				    }

				    var oldSkin = Skin;
				    Skin = rp.Skin;

				   
			    }*/
			    
			    StealSkin(entity);
			    return;
		    }

		  //  Log.Info($"Interacting with entity. Attack: {attack} - CanAttack: {canAttack} - PVM: {IsNoPvM} - PVP: {IsNoPvP}");
		  var slot = hand == 1 ? Inventory.OffHandSlot : Inventory.SelectedSlot;
		  var interaction = ItemUseOnEntityAction.ItemInteract;
		  
		    if (attack)
		    {
			    interaction = ItemUseOnEntityAction.Attack;
		    }
		    else
		    {
			    interaction = ItemUseOnEntityAction.Interact;
			    //Network?.EntityInteraction(this, entity, ItemUseOnEntityAction.Interact, hand, slot);
		    }
		    
		    Network?.EntityInteraction(this, entity, interaction, hand, slot, cursorPosition);
	    }

	    private void StealSkin(Entity sourceEntity)
	    {
		    if (Network is BedrockClient bc)
		    {
			    MiNET.Utils.Skins.Skin skin = null;
			    
			    if (sourceEntity is RemotePlayer player)
			    {
				    if (player.Skin != null)
					    skin = player.Skin;
			    }
			   /* else
			    {
				    if (sourceEntity?.ModelRenderer?.Model == null)
					    return;
				    
				    var model   = sourceEntity.ModelRenderer.Model;
				    skin = model.ToSkin();
			    }
*/
			    if (skin == null)
				    return;

			    var texture = sourceEntity.Texture;

			    if (skin.Data == null || skin.Data.Length == 0)
			    {
				    skin = skin.UpdateTexture(texture);
			    }

			    var packet = McpePlayerSkin.CreateObject();
			    packet.skin = skin;

			    packet.uuid = UUID;
			    packet.isVerified = true;
			    packet.skinName = skin.SkinId;
			    packet.oldSkinName = "";

			    bc.SendPacket(packet);

			    Skin = skin;
			    Log.Info($"Stole skin from {sourceEntity.NameTag}");
		    }
		    
	    }
	    
	    public Entity HitEntity { get; private set; } = null;
	    private Vector3	EntityTracerPoint { get; set; } = Vector3.Zero;
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
				    EntityTracerPoint = targetPoint;
				    hitEntity = entity;
				    break;
			    }
		    }

		    HitEntity = hitEntity;
	    }

	    public Block   SelBlock              { get; private set; } = null;
	    public Vector3 RaytracedBlock        { get; private set; }
	    public Vector3 AdjacentRaytraceBlock { get; private set; }

	    public  BoundingBox[]     RaytraceBoundingBoxes => _boundingBoxes.ToArray();
	    private List<BoundingBox> _boundingBoxes = new List<BoundingBox>();
	    private void UpdateBlockRayTracer()
	    {
		    var camPos     = Level.Camera.Position;
		    var lookVector = Level.Camera.Direction;

		    //List<BoundingBox> boundingBoxes = new List<BoundingBox>();
		   // var               ray           = new Ray(camPos, lookVector * 8f);
		    
		    for (float x = (float) (Width * Scale); x < 8f; x += 0.01f)
		    {
			    Vector3 targetPoint  = camPos + (lookVector * x);
			    var     flooredBlock = Vector3.Floor(targetPoint);
			    var     block        = Level.GetBlockState(targetPoint);

			    if (block != null && block.Block.HasHitbox)
			    {
				    //boundingBoxes.Clear();

				    var boundingBoxes = block.Block.GetBoundingBoxes(flooredBlock).ToArray();

				    foreach (var bbox in boundingBoxes)
				    {
					    if (bbox.Contains(targetPoint) == ContainmentType.Contains)
					    {
						    _boundingBoxes.Clear();
						    
						    RaytracedBlock = Vector3.Floor(targetPoint);
						    SelBlock = block.Block;
						    //  RayTraceBoundingBox = bbox;

						    _raytraced = targetPoint;
						    HasRaytraceResult = true;
						    _boundingBoxes.AddRange(boundingBoxes);

						    if (SetPlayerAdjacentSelectedBlock(Level, x, camPos, lookVector, out Vector3 rawAdjacent))
						    {
							    AdjacentRaytraceBlock = Vector3.Floor(rawAdjacent);
							    _adjacentRaytrace = rawAdjacent;
						    }
						    
						    return;
					    }
				    }
			    }
		    }

		    SelBlock = null;
		    HasRaytraceResult = false;
		    _boundingBoxes.Clear();
	    }
	    
	    private bool SetPlayerAdjacentSelectedBlock(World world, float xStart, Vector3 camPos, Vector3 lookVector, out Vector3 rawAdjacent)
	    {
		    for (float x = xStart; x > 0.7f; x -= 0.1f)
		    {
			    Vector3 targetPoint = camPos + (lookVector * x);
			    var     blockState  = world.GetBlockState(targetPoint);

			    if (blockState != null && (!blockState.Block.Solid))
			    {
				    rawAdjacent = targetPoint;
				    return true;
			    }
		    }
		    
		    rawAdjacent = new Vector3(0, 0, 0);
		    return false;
	    }

	    public void DropHeldItem(bool fullStack = false)
	    {
		    var floored = new BlockCoordinates(Vector3.Floor(_raytraced));
		    var face    = GetTargetFace();
		    
		    //var adjacent = _adjacentRaytrace;
		    //var flooredAdj = Vector3.Floor(adjacent);
		    //var remainder = new Vector3(adjacent.X - flooredAdj.X, adjacent.Y - flooredAdj.Y, adjacent.Z - flooredAdj.Z);

		    var item = Inventory.MainHand;
		    Network?.DropItem(floored, face, item, fullStack);
		    
		    if (Gamemode != GameMode.Creative)
		    {
			    item.Count -= 1;
			    if (fullStack || item.Count <= 0)
			    {
				    item = new ItemAir() {Count = 0};
			    }

			    Inventory.MainHand = item;
		    }
	    }
	    
	    private void BlockBreakTick()
	    {
		    var tick =  Interlocked.Increment(ref _destroyingTick);
		    if (tick % 10 == 0)
		    {
			    SwingArm(true);
		    }

		   // Level?.SetBlockBreakProgress(_destroyingTarget, BlockBreakProgress);
		    
		    if (tick >= _destroyTimeNeeded)
		    {
			    StopBreakingBlock(true);
		    }
	    }

	    private void StartBreakingBlock()
	    {
		    SwingArm(true);
		    
			var floored  = new BlockCoordinates(Vector3.Floor(_raytraced));
			var adjacent = _adjacentRaytrace;
			
		    var blockState = Level.GetBlockState(floored);
		    var block      = blockState.Block;
		    if (!block.HasHitbox)
		    {
			    return;
		    }

		    var face = GetTargetFace();

		    _destroyingBlock = true;
		    _destroyingTarget = floored;
		    _destroyingFace = face;
		    
		    Interlocked.Exchange(ref _destroyingTick, 0);
		    
		    _destroyTimeNeeded = block.GetBreakTime(Inventory.MainHand ?? new ItemAir()) * 20f;

            var flooredAdj = Vector3.Floor(adjacent);
            var remainder = new Vector3(adjacent.X - flooredAdj.X, adjacent.Y - flooredAdj.Y, adjacent.Z - flooredAdj.Z);

            Network?.PlayerDigging(DiggingStatus.Started, floored, face, remainder);
            
            Level?.AddOrUpdateBlockBreak(_destroyingTarget, _destroyTimeNeeded);
        }

	    private void StopBreakingBlock(bool sendToServer = true, bool forceCanceled = false)
	    {
		    if (!_destroyingBlock)
			    return;
		    
		    _destroyingBlock = false;

		    var ticks = Interlocked.Exchange(ref _destroyingTick, 0);// = 0;

            var flooredAdj = Vector3.Floor(_adjacentRaytrace);
            var remainder = new Vector3(_adjacentRaytrace.X - flooredAdj.X, _adjacentRaytrace.Y - flooredAdj.Y, _adjacentRaytrace.Z - flooredAdj.Z);
            
            Level?.EndBreakBlock(_destroyingTarget);
            
            if (!sendToServer)
		    {
			    return;
		    }

		    if ((Gamemode == GameMode.Creative  || ticks >= _destroyTimeNeeded) && !forceCanceled)
		    {
                Network?.PlayerDigging(DiggingStatus.Finished, _destroyingTarget, _destroyingFace, remainder);
                Level?.SetBlockState(_destroyingTarget, new Air().BlockState);
            }
		    else
		    {
			    Network?.PlayerDigging(DiggingStatus.Cancelled, _destroyingTarget, _destroyingFace, remainder);
            }
	    }

	    public void CancelBlockBreaking()
	    {
		    StopBreakingBlock(true, true);
	    }
	    
	    private BlockFace GetTargetFace()
	    {
		    var flooredAdj =  Vector3.Floor(_adjacentRaytrace);
		    var raytraceFloored  = Vector3.Floor(_raytraced);

		    var adj = flooredAdj - raytraceFloored;
		    adj.Normalize();

		    return adj.GetBlockFace();
        }

	    private void HandleLeftClick(Item slot, int hand)
	    {
		    HandleClick(slot, hand, Inventory.HotbarOffset + Inventory.SelectedSlot, false, true);
	    }

	    private bool HandleClick(Item slot,
		    int hand,
		    int inventorySlot,
		    bool canModifyWorld = true,
		    bool isLeftClick = false)
	    {
		    //  Log.Info($"Clicky clicky click. Left click: {isLeftClick} Can modify world: {canModifyWorld} HasRaytrace: {HasRaytraceResult}");
		    SwingArm(true);

		    var flooredAdj = Vector3.Floor(_adjacentRaytrace);
		    var raytraceFloored = Vector3.Floor(_raytraced);

		    var adj = flooredAdj - raytraceFloored;
		    adj.Normalize();

		    var face = adj.GetBlockFace();

		    var remainder = new Vector3(
			    _adjacentRaytrace.X - flooredAdj.X, _adjacentRaytrace.Y - flooredAdj.Y,
			    _adjacentRaytrace.Z - flooredAdj.Z);

		    var coordR = new BlockCoordinates(raytraceFloored);

		    if (HasRaytraceResult)
		    {
			    var existingBlockState = Level.GetBlockState(coordR);
			    var existingBlock = existingBlockState.Block;

			    if (existingBlock.CanInteract && ((!(slot is ItemBlock) || isLeftClick) || IsSneaking))
			    {
				    if (!existingBlock.Interact(this, slot, remainder))
				    {
					    Network?.WorldInteraction(this, coordR, face, hand, inventorySlot, remainder);
				    }

				    return true;
			    }

			    if (slot is ItemBlock ib && canModifyWorld && !isLeftClick)
			    {
				    BlockState blockState = ib.Block;

				    if (blockState != null && !(blockState.Block is Air) && HasRaytraceResult)
				    {
					    BlockCoordinates target;
					    if (existingBlock.BlockMaterial.IsReplaceable)
					    {
						    target = coordR;
					    }
					    else
					    {
						    target = new BlockCoordinates(raytraceFloored + adj);
					    }
					    
					    if (CanPlaceBlock(target, (Block) blockState.Block))
					    {
						    Level.SetBlockState(target, blockState);
						    Network?.BlockPlaced(coordR, face, hand, inventorySlot, remainder, this);
						    
						    return true;
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

			    Network?.UseItem(slot, hand, action, coordR, face, remainder);

			    return true;
		    }

		    return false;
	    }

	    private void BeginUseItem(bool isLeftMouseButton)
	    {
		    var item = Inventory.MainHand;

		    if (item != null && item.Count > 0 && item.Id > 0)
		    {
			    IsUsingItem = true;
		    }
	    }

	    private void StopUseItem()
	    {
		    IsUsingItem = false;
	    }

	    private bool CanPlaceBlock(BlockCoordinates coordinates, Block block)
	    {
		    var bb = block.GetBoundingBoxes(coordinates);
		    var playerBb = BoundingBox;

		    foreach (var boundingBox in bb)
		    {
			    if (playerBb.Intersects(boundingBox))
			    {
				    return false;
			    }
		    }

		    return true;
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

	    /// <inheritdoc />
	    public override int Render(IRenderArgs renderArgs, bool useCulling)
	    {
		    int renderCount = 0;

		    if (IsFirstPersonMode)
		    {
			    var renderer = ItemRenderer;

			    if (renderer != null)
			    {
				    renderCount += renderer.Render(
					    renderArgs, null, Matrix.CreateScale(Scale / 16f) * RenderLocation.CalculateWorldMatrix());
			    }
		    }
		    else
		    {
			    var renderer = ModelRenderer;

			    if (renderer != null)
			    {
				    renderCount += renderer.Render(
					    renderArgs, useCulling, Effect,
					    Matrix.CreateScale((1f / 16f) * Scale) * RenderLocation.CalculateWorldMatrix());
			    }
		    }

		    return renderCount;
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

	    /// <inheritdoc />
	    public override void Jump()
	    {
		    base.Jump();
		    Network?.EntityAction((int) EntityId, EntityAction.Jump);
	    }

	    /// <inheritdoc />
	    public override void SwingArm(bool broadcast, bool leftHanded)
	    {
		    base.SwingArm(broadcast, leftHanded);
		    
		    if (broadcast)
		    {
			    Network?.PlayerAnimate(leftHanded ? PlayerAnimations.SwingLeftArm : PlayerAnimations.SwingRightArm);
		    }
	    }
    }
}