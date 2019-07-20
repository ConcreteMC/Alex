using System;
using System.Linq;
using Alex.API.Blocks.State;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Input;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.GameStates.Playing;
using Alex.Items;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Net;
using NLog;
using Inventory = Alex.Utils.Inventory;

namespace Alex.Entities
{
    public class Player : PlayerMob
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Player));

        public static readonly float EyeLevel = 1.625F;

		public PlayerIndex PlayerIndex { get; }

		public float FOVModifier { get; set; } = 0;

		public PlayerController Controller { get; }
        public Vector3 Raytraced = Vector3.Zero;
        public Vector3 AdjacentRaytrace = Vector3.Zero;
        public bool HasAdjacentRaytrace = false;
        public bool HasRaytraceResult = false;

        public Player(GraphicsDevice graphics, Alex alex, string name, World world, Skin skin, INetworkProvider networkProvider, PlayerIndex playerIndex) : base(name, world, networkProvider, skin.Texture, true)
		{
		//	DoRotationCalculations = false;
			PlayerIndex = playerIndex;
		    Controller = new PlayerController(graphics, world, alex.InputManager, this, playerIndex); 
		    NoAi = false;

			Inventory = new Inventory(46);
			Inventory.SelectedHotbarSlotChanged += SelectedHotbarSlotChanged;
			MovementSpeed = 4.317f;
			FlyingSpeed = 10.89f;

			SnapHeadYawRotationOnMovement = false;

			RenderEntity = true;
		}

	    private void SelectedHotbarSlotChanged(object sender, SelectedSlotChangedEventArgs e)
	    {
		    Network?.HeldItemChanged(e.NewValue);
	    }

		private BlockCoordinates _destroyingTarget = BlockCoordinates.Zero;
	    private bool _destroyingBlock = false;
        private DateTime _destroyingTick = DateTime.MaxValue;
	    private double _destroyTimeNeeded = 0;
	    private BlockFace _destroyingFace;

	    public override void Update(IUpdateArgs args)
		{
			ChunkCoordinates oldChunkCoordinates = new ChunkCoordinates(base.KnownPosition);
			bool sprint = IsSprinting;
			bool sneak = IsSneaking;

			if (Controller.IsFreeCam && !CanFly)
			{
				Controller.IsFreeCam = false;
			}
			else if (CanFly)
			{
				IsFlying = Controller.IsFreeCam;
			}
			
			Controller.Update(args.GameTime);
			//KnownPosition.HeadYaw = KnownPosition.Yaw;

			if (IsSprinting && !sprint)
			{
				FOVModifier += 10;
				
				Network.EntityAction((int) EntityId, EntityAction.StartSprinting);
			}
			else if (!IsSprinting && sprint)
			{
				FOVModifier -= 10;
				
				Network.EntityAction((int)EntityId, EntityAction.StopSprinting);
			}

			if (IsSneaking != sneak)
			{
				if (IsSneaking)
				{
					Network.EntityAction((int)EntityId, EntityAction.StartSneaking);		
				}
				else
				{
					Network.EntityAction((int)EntityId, EntityAction.StopSneaking);
				}
			}

			if (Controller.CheckInput)
			{
				var hitEntity = HitEntity;
				if (hitEntity != null && Controller.InputManager.IsPressed(InputCommand.LeftClick))
				{
					if (_destroyingBlock)
						StopBreakingBlock(forceCanceled:true);
					
					InteractWithEntity(hitEntity, true);
				}
				else if (hitEntity != null && Controller.InputManager.IsPressed(InputCommand.RightClick))
				{
					if (_destroyingBlock)
						StopBreakingBlock(forceCanceled:true);
					
					InteractWithEntity(hitEntity, false);
				}
				else if (hitEntity == null && !_destroyingBlock && Controller.InputManager.IsDown(InputCommand.LeftClick)) //Destroying block.
				{
					StartBreakingBlock();
				}
				else if (_destroyingBlock && Controller.InputManager.IsUp(InputCommand.LeftClick))
				{
					StopBreakingBlock();
				}
				else if (_destroyingBlock && Controller.InputManager.IsDown(InputCommand.LeftClick))
				{
					if (_destroyingTarget != new BlockCoordinates(Raytraced.Floor()))
					{
						StopBreakingBlock(true, true);

						if (Gamemode != Gamemode.Creative)
						{
							StartBreakingBlock();
						}
					}
				}
				else if (Controller.InputManager.IsPressed(InputCommand.RightClick))
				{
					bool handledClick = false;
					// Log.Debug($"Right click!");
					if (Inventory.MainHand != null && !(Inventory.MainHand is ItemAir))
					{
						handledClick = HandleRightClick(Inventory.MainHand, 0);
					}

					if (!handledClick && Inventory.OffHand != null && !(Inventory.OffHand is ItemAir))
					{
						handledClick = HandleRightClick(Inventory.OffHand, 1);
					}

					if (!handledClick)
					{
						var flooredAdj = AdjacentRaytrace.Floor();
						var remainder = new Vector3(AdjacentRaytrace.X - flooredAdj.X, AdjacentRaytrace.Y - flooredAdj.Y, AdjacentRaytrace.Z - flooredAdj.Z);
						Network?.BlockPlaced(Raytraced, GetTargetFace(), 0, remainder);

						handledClick = true;
					}
				}
            }
			else if (_destroyingBlock)
			{
				StopBreakingBlock();
			}
			
			UpdateRayTracer();

            base.Update(args);

		}

	    private void InteractWithEntity(IEntity entity, bool attack)
	    {
		    if (attack)
		    {
			    Network?.EntityInteraction(this, entity, McpeInventoryTransaction.ItemUseOnEntityAction.Attack);
		    }
		    else
		    {
			    Network?.EntityInteraction(this, entity, McpeInventoryTransaction.ItemUseOnEntityAction.Interact);
		    }
	    }

	    public IEntity HitEntity { get; private set; } = null;
	    public IEntity[] EntitiesInRange { get; private set; } = null;
	    
	    private void UpdateRayTracer()
	    {
		    var camPos = Level.Camera.Position;
		    var lookVector = Level.Camera.Direction;

		    var entities = Level.EntityManager.GetEntities(camPos, 8);
		    EntitiesInRange = entities.ToArray();
		    
		    if (EntitiesInRange.Length == 0) return;
		    
		    IEntity hitEntity = null;
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
			var floored = Raytraced.Floor();

		    var block = Level.GetBlock(floored);
		    if (!block.HasHitbox)
		    {
			    return;
		    }

            _destroyingBlock = true;
		    _destroyingTarget = floored;
		    _destroyingFace = GetTargetFace();
		    _destroyingTick = DateTime.UtcNow;

		    if (Inventory.MainHand == null) return;
		    _destroyTimeNeeded = block.GetBreakTime(Inventory.MainHand);

            Log.Debug($"Start break block ({_destroyingTarget}, {_destroyTimeNeeded} seconds.)");

            var flooredAdj = AdjacentRaytrace.Floor();
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

		    var timeRan = (end - start).TotalSeconds;

            var flooredAdj = AdjacentRaytrace.Floor();
            var remainder = new Vector3(AdjacentRaytrace.X - flooredAdj.X, AdjacentRaytrace.Y - flooredAdj.Y, AdjacentRaytrace.Z - flooredAdj.Z);

            if (!sendToServer)
		    {
			    Log.Debug($"Stopped breaking block, not notifying server. Time: {timeRan}");
                return;
		    }

		    if ((Gamemode == Gamemode.Creative  || timeRan >= _destroyTimeNeeded) && !forceCanceled)
		    {
                Network?.PlayerDigging(DiggingStatus.Finished, _destroyingTarget, _destroyingFace, remainder);
			    Log.Debug($"Stopped breaking block. Time: {timeRan}");

				Level.SetBlockState(_destroyingTarget, new Air().GetDefaultState());
            }
		    else
		    {
			    Network?.PlayerDigging(DiggingStatus.Cancelled, _destroyingTarget, _destroyingFace, remainder);
			    Log.Debug($"Cancelled breaking block. Time: {timeRan}");
            }
	    }

	    private BlockFace GetTargetFace()
	    {
		    var flooredAdj = AdjacentRaytrace.Floor();
		    var raytraceFloored = Raytraced.Floor();

		    var adj = flooredAdj - raytraceFloored;
		    adj.Normalize();

		    return adj.GetBlockFace();
        }

	    private bool HandleRightClick(Item slot, int hand)
	    {
		    //if (ItemFactory.ResolveItemName(slot.ItemID, out string itemName))
            {
	            IBlockState blockState = null;
	         //   if (ItemFactory.TryGetItem(itemName, out Item i))
	         //   {
		            if (slot is ItemBlock ib)
		            {
			            blockState = ib.Block;
		            }
	         //   }

                if (blockState != null && !(blockState.Block is Air) && HasRaytraceResult)
                {
                    var flooredAdj = AdjacentRaytrace.Floor();
	                var raytraceFloored = Raytraced.Floor();

                    var adj = flooredAdj - raytraceFloored;
                    adj.Normalize();

	                var face = adj.GetBlockFace();

                    var remainder = new Vector3(AdjacentRaytrace.X - flooredAdj.X, AdjacentRaytrace.Y - flooredAdj.Y, AdjacentRaytrace.Z - flooredAdj.Z);

	                var coordR = new BlockCoordinates(raytraceFloored);
                    Network?.BlockPlaced(coordR, face, hand, remainder);

                    var existingBlock = Level.GetBlock(coordR);
	                if (existingBlock.IsReplacible || !existingBlock.Solid)
	                {
		                Level.SetBlockState(coordR, blockState);
                    }
	                else
	                {
		                Level.SetBlockState(new BlockCoordinates(raytraceFloored + adj), blockState);
	                }

	                Log.Debug($"Placed block: {slot.DisplayName} on {raytraceFloored} face= {face} facepos={remainder} ({adj})");

	                return true;
                }
                else if (blockState == null)
                {
                    Network?.UseItem(hand);
                    Log.Debug($"Used item!");

	                return true;
                }
            }

		    return false;
	    }

		public override void TerrainCollision(Vector3 collisionPoint, Vector3 direction)
		{
		//	Log.Debug($"Terrain collision: {collisionPoint.ToString()} | {direction}");	
			base.TerrainCollision(collisionPoint, direction);
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