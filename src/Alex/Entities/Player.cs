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
using Alex.Entities.Components;
using Alex.Entities.Projectiles;
using Alex.Gamestates;
using Alex.Gamestates.InGame;
using Alex.Gui.Elements.Map;
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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
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

		public PlayerController Controller { get; }
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
        
        public override bool IsSprinting
        {
	        get => base.IsSprinting;
	        set
	        {
				
	        }
        }
        
        /// <inheritdoc />
        public override void SetSwimming(bool swimming)
        {
	        if (swimming && !IsSwimming)
	        {
		        IsSwimming = true;
		        Network?.EntityAction((int)EntityId, EntityAction.StartSwimming);
	        }
	        else if (!swimming && IsSwimming)
	        {
		        IsSwimming = false;
		        Network?.EntityAction((int)EntityId, EntityAction.StopSwimming);
	        }
        }

        /// <inheritdoc />
        public override void SetFlying(bool flying)
        {
	        if (!CanFly && flying)
	        {
		        IsFlying = false;
		        return;
	        }
	        if (flying && !IsFlying)
	        {
		        IsFlying = true;
		        Network?.EntityAction((int)EntityId, EntityAction.StartFlying);
	        }
	        else if (!flying && IsFlying)
	        {
		        IsFlying = false;
		        Network?.EntityAction((int)EntityId, EntityAction.StopFlying);
	        }
        }

        /// <inheritdoc />
        public override void SetSprinting(bool sprinting)
        {
	        bool wasSprinting = IsSprinting;
	        base.SetSprinting(sprinting);

	        /*if (wasSprinting && !IsSprinting)
	        {
		        Network?.EntityAction((int)EntityId, EntityAction.StopSprinting);
	        }
	        else if (!wasSprinting && IsSprinting)
	        {
		        Network?.EntityAction((int)EntityId, EntityAction.StartSprinting);
	        }*/
        }

       // public BlockCoordinates SpawnPoint { get; set; } = BlockCoordinates.Zero;

        public RaytracerComponent Raytracer { get; }
        //public Camera Camera { get; internal set; }
        public Player(GraphicsDevice graphics, InputManager inputManager, World world, NetworkProvider networkProvider, PlayerIndex playerIndex) : base(world)
        {
	        Network = networkProvider;
	        
		    Controller = new PlayerController(graphics, inputManager, this, playerIndex);

		    SnapHeadYawRotationOnMovement = false;
			SnapYawRotationOnMovement = true;
			DoRotationCalculations = false;
			
			RenderEntity = true;

			IsAffectedByGravity = true;
			HasPhysics = true;
			base.NoAi = false;
			CanSwim = true;
			
			EntityComponents.Push(Raytracer = new RaytracerComponent(this));
			
			base.MapIcon = new LocalPlayerMapIcon(this, MapMarker.GreenPointer);
        }

        /// <inheritdoc />
        public override void OnDespawn()
        {
	        base.OnDespawn();

	        if (Network is BedrockClient bc)
	        {
		        bc.WorldProvider.FormManager.CloseAll();
		        var activeDialog = Alex.Instance.GuiManager.ActiveDialog;

		        if (activeDialog != null && activeDialog is not DisconnectedDialog)
		        {
			        Alex.Instance.GuiManager.HideDialog(activeDialog);
		        }
	        }
        }

        /// <inheritdoc />
        protected override void OnSkinValueChanged(MiNET.Utils.Skins.Skin newSkin)
        {
			//Do nothing.
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
	    private bool _skipUpdate = false;

	    internal void SkipUpdate()
	    {
		    _skipUpdate = true;
	    }

	    /// <inheritdoc />
	    protected override void OnSneakingChanged(bool newValue)
	    {
		    if (newValue)
		    {
			    Network.EntityAction((int) EntityId, EntityAction.StartSneaking);
			    Level.Camera.UpdateOffset(new Vector3(0f, -0.125F, 0f));
		    }
		    else
		    {
			    Network.EntityAction((int) EntityId, EntityAction.StopSneaking);
			    Level.Camera.UpdateOffset(Vector3.Zero);
		    }
	    }

	    /// <inheritdoc />
	    protected override void OnSprintingChanged(bool newValue)
	    {
		    if (newValue)
		    {
			    FOVModifier = 10;
			    Network.EntityAction((int) EntityId, EntityAction.StartSprinting);
		    }
		    else
		    {
			    FOVModifier = 0;
			    Network.EntityAction((int) EntityId, EntityAction.StopSprinting);
		    }
	    }

	    private bool _previousHasActiveDialog = false;
	    public Biome CurrentBiome { get; private set; }
	    public override void Update(IUpdateArgs args)
	    {
		    base.Update(args);

		    if (!IsSpawned)
			    return;

		    CurrentBiome = Level.GetBiome(RenderLocation.GetCoordinates3D());
		    
		    if (!CanFly && IsFlying)
			    IsFlying = false;

		    if (IsSprinting && !CanSprint)
		    {
			    SetSprinting(false);
			   // IsSprinting = false;
		    }
		    
		    Controller.Update(args.GameTime);

		    if (_skipUpdate)
		    {
			    if (Controller.CheckInput && Controller.CheckMovementInput)
			    {
				    _skipUpdate = false;
			    }
		    }
		    else if ((Controller.CheckInput && Controller.CheckMovementInput && !_previousHasActiveDialog && !_skipUpdate))
		    {
			    UpdateRayTracer();

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
				    //if (hitEntity is LivingEntity)
				    {
					    if (didLeftClick || didRightClick)
					    {
						    if (_destroyingBlock)
							    StopBreakingBlock(forceCanceled: true);

						    var adjacent = EntityTracerPoint;
						    var flooredAdj = Vector3.Floor(adjacent);
						    var remainder = new Vector3(adjacent.X - flooredAdj.X, adjacent.Y - flooredAdj.Y, adjacent.Z - flooredAdj.Z);
						    
						    InteractWithEntity(hitEntity, didLeftClick, IsLeftHanded ? 1 : 0, remainder);
						    
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
					    else if (_destroyingTarget != Raytracer.ResultingCoordinates)
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
					    if (Raytracer.HasValue)
					    {
						    if (beginLeftClick && !IsWorldImmutable)
						    {
							    StartBreakingBlock();
						    }
					    }
					    else if (didLeftClick)
					    {
						    HandleLeftClick(
							    IsLeftHanded ? Inventory.OffHand : Inventory.MainHand, IsLeftHanded ? 1 : 0);
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
						    item, IsLeftHanded ? 1 : 0, IsLeftHanded ? Inventory.OffHandSlot : (Inventory.HotbarOffset + Inventory.SelectedSlot));
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

		    //if (FeetInWater && HeadInWater)
			//    IsSwimming = true;
		    //else
			//    IsSwimming = false;
	    }

	    private void InteractWithEntity(Entity entity, bool attack, int hand, Vector3 cursorPosition)
	    {
		    if ((entity is ItemEntity))
			    return;
		    
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
		    if (Network is not BedrockClient bc) return;
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
		    
		    if (skin.Data == null || skin.Data.Length == 0)
		    {
			    var texture = sourceEntity.Texture;
			    Image<Rgba32> skinTexture;
			    using (MemoryStream ms = new MemoryStream())
			    {
				    texture.SaveAsPng(ms, texture.Width, texture.Height);
				    ms.Position = 0;

				    skinTexture = Image.Load(ms, new PngDecoder()).CloneAs<Rgba32>();
			    }
				    
			    skin = skin.UpdateTexture(skinTexture);
		    }

		    var packet = McpePlayerSkin.CreateObject();
		    packet.skin = skin;

		    packet.uuid = UUID;
		    packet.isVerified = skin.IsVerified;
		    packet.skinName = skin.SkinId;
		    packet.oldSkinName = Skin?.SkinId ?? "";

		    bc.SendPacket(packet);

		    Skin = skin;
		    Log.Info($"Stole skin from {sourceEntity.NameTag}");

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

	    public void DropHeldItem(bool fullStack = false)
	    {
		    var floored = Raytracer.ResultingCoordinates;
		    var face = Raytracer.Face;

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
		    
		    var blockState = Level.GetBlockState(Raytracer.ResultingCoordinates);
		    var block      = blockState.Block;
		    if (!block.HasHitbox)
		    {
			    return;
		    }

		    var face = Raytracer.Face;

		    _destroyingBlock = true;
		    _destroyingTarget = Raytracer.ResultingCoordinates;
		    _destroyingFace = face;
		    
		    Interlocked.Exchange(ref _destroyingTick, 0);
		    
		    _destroyTimeNeeded = (block.GetBreakTime(Inventory.MainHand ?? new ItemAir()) * AttackSpeed) * 20f;

            Network?.PlayerDigging(DiggingStatus.Started, Raytracer.ResultingCoordinates, face, Raytracer.CursorPosition);

            Level?.AddOrUpdateBlockBreak(_destroyingTarget, _destroyTimeNeeded);

            if ((Gamemode == GameMode.Creative))
            {
	            StopBreakingBlock(true, false);
            }
        }

	    private void StopBreakingBlock(bool sendToServer = true, bool forceCanceled = false)
	    {
		    if (!_destroyingBlock)
			    return;
		    
		    _destroyingBlock = false;

		    var ticks = Interlocked.Exchange(ref _destroyingTick, 0);// = 0;
		    
            Level?.EndBreakBlock(_destroyingTarget);
            
            if (!sendToServer)
		    {
			    return;
		    }

		    if ((Gamemode == GameMode.Creative  || ticks >= _destroyTimeNeeded) && !forceCanceled)
		    {
                Network?.PlayerDigging(DiggingStatus.Finished, _destroyingTarget, _destroyingFace, Raytracer.CursorPosition);
                Level?.SetBlockState(_destroyingTarget, new Air().BlockState);
            }
		    else
		    {
			    Network?.PlayerDigging(DiggingStatus.Cancelled, _destroyingTarget, _destroyingFace, Raytracer.CursorPosition);
            }
	    }

	    public void CancelBlockBreaking()
	    {
		    StopBreakingBlock(true, true);
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
		    
		    var face = Raytracer.Face;

		    var blockPosition = Raytracer.ResultingCoordinates;

		    var hitEntity = HitEntity;

		    if (hitEntity != null)
		    {
			    if (isLeftClick)
			    {
				    
			    }
		    }
		    
		    if (Raytracer.HasValue)
		    {
			    var existingBlockState = Level.GetBlockState(blockPosition);
			    var existingBlock = existingBlockState.Block;

			    if (existingBlock.CanInteract && (!isLeftClick && !IsSneaking))
			    {
				    if (!existingBlock.Interact(this, slot, Raytracer.CursorPosition))
				    {
					    Network?.WorldInteraction(this, blockPosition, face, hand, inventorySlot, Raytracer.CursorPosition);
					    Log.Info($"Sending world interaction. Block={existingBlock} Face={face} Coordinates={blockPosition}");
				    }
				    return true;
			    }

			    if (slot is ItemBlock ib && canModifyWorld && !isLeftClick)
			    {
				    BlockState blockState = ib.Block;

				    if (blockState != null && !slot.IsAir())
				    {
					    if (CanPlaceBlock(blockPosition, (Block) blockState.Block))
					    {
						    if (blockState.Block.PlaceBlock(Level, this, blockPosition, face, Raytracer.CursorPosition))
						    {
							    Network?.BlockPlaced(blockPosition, face, hand, inventorySlot, Raytracer.CursorPosition, this);
							    return true;
						    }
					    }
					    else
					    {
						    Log.Warn($"Tried placing invalid");
					    }
				    }
			    }
			    else
			    {
				    Log.Warn($"Can ModifyWorld: {canModifyWorld} | IsLeftClick={isLeftClick} | ItemBlock={slot.Name} | ItemType={slot.GetType()}");
			    }
		    }

		    if (!slot.IsAir()  && slot.Id > 0 && slot.Count > 0)
		    {
			    ItemUseAction action;

			    if (isLeftClick)
			    {
				    action = Raytracer.HasValue ? ItemUseAction.ClickBlock : ItemUseAction.ClickAir;
			    }
			    else
			    {
				    action = Raytracer.HasValue ? ItemUseAction.RightClickBlock : ItemUseAction.RightClickAir;
			    }

			    Network?.UseItem(slot, hand, action, blockPosition, face, Raytracer.CursorPosition);
			    Log.Info($"Using item, action={action}");
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
			    Level.Camera.FieldOfViewOffset = value;
		    }
	    }

	    /// <inheritdoc />
	    public override int Render(IRenderArgs renderArgs, bool useCulling)
	    {
		    int renderCount = 0;

		    var worldMatrix = Matrix.CreateScale((1f / 16f) * (Scale * ModelScale))
		                      * RenderLocation.CalculateWorldMatrix();

		    var renderer = ModelRenderer;

		    if (renderer != null)
		    {
			    if (!IsFirstPersonMode)
				    renderCount += renderer.Render(renderArgs, worldMatrix);

			    renderCount += RenderHeldItem(renderArgs, renderer, worldMatrix);
		    }

		    return renderCount;
	    }

	    //private vector
	    public override void OnTick()
		{
			if (WaitingOnChunk)
			{
				//  NoAi = true;

				if (Level.GetChunk(KnownPosition.GetCoordinates3D(), true) != null)
				{
					WaitingOnChunk = false;
					// NoAi = false;
				}
			}
			
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
			
			Controller?.Tick();
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

	    /// <inheritdoc />
	    protected override void OnDispose()
	    {
		    base.OnDispose();
		    
		    Controller?.Dispose();
	    }
    }
}