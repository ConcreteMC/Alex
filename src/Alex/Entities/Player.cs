using System;
using System.Threading;
using Alex.API.Blocks.State;
using Alex.API.Data;
using Alex.API.Graphics;
using Alex.API.Input;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.GameStates.Playing;
using Alex.Graphics.Models.Entity;
using Alex.Items;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using NLog;
using Inventory = Alex.Utils.Inventory;
using Ray = MiNET.Utils.Ray;

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
			DoRotationCalculations = false;
			PlayerIndex = playerIndex;
		    Controller = new PlayerController(graphics, world, alex.GameSettings, alex.InputManager, this, playerIndex); 
		    NoAi = false;

			Inventory = new Inventory(46);
			Inventory.SelectedHotbarSlotChanged += SelectedHotbarSlotChanged;
			MovementSpeed = 0.1;
		}

	    private void SelectedHotbarSlotChanged(object sender, SelectedSlotChangedEventArgs e)
	    {
		    Network?.HeldItemChanged(e.NewValue);
	    }

		private BlockCoordinates _destroyingTarget = BlockCoordinates.Zero;
	    private bool _destroyingBlock = false;
        private int _destroyingTick = 0;
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
			KnownPosition.HeadYaw = KnownPosition.Yaw;

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
				if (!_destroyingBlock && Controller.InputManager.IsDown(InputCommand.LeftClick)) //Destroying block.
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
						StartBreakingBlock();
					}
				}

				if (Controller.InputManager.IsPressed(InputCommand.RightClick))
				{
					bool handledClick = false;
					// Log.Debug($"Right click!");
					if (Inventory.MainHand != null && Inventory.MainHand.ItemID != -1 && Inventory.MainHand.ItemID != 0)
					{
						handledClick = HandleRightClick(Inventory.MainHand, 0);
					}

					if (!handledClick && Inventory.OffHand != null && Inventory.OffHand.ItemID != -1 &&
					         Inventory.OffHand.ItemID != 0)
					{
						handledClick = HandleRightClick(Inventory.OffHand, 1);
					}

				}
            }
			else if (_destroyingBlock)
			{
				StopBreakingBlock();
			}

            base.Update(args);

		}

	    private void BlockBreakTick()
	    {
		    _destroyingTick++;
        }

	    private void StartBreakingBlock()
	    {
			var floored = Raytraced.Floor();

		    if (!Level.GetBlock(floored).HasHitbox)
		    {
			    return;
		    }

            _destroyingBlock = true;
		    _destroyingTarget = floored;
		    _destroyingFace = GetTargetFace();

            Log.Debug($"Start break block ({_destroyingTarget})");

            Network?.PlayerDigging(DiggingStatus.Started, _destroyingTarget, _destroyingFace);
        }

	    private void StopBreakingBlock(bool sendToServer = true, bool forceCanceled = false)
	    {
		    _destroyingBlock = false;
            var ticks = Interlocked.Exchange(ref _destroyingTick, 0);// = 0;

		    if (!sendToServer)
		    {
			    Log.Debug($"Stopped breaking block, not notifying server. Ticks: {ticks}");
                return;
		    }

		    if ((Gamemode == Gamemode.Creative /* || ticks >= block.MiningTime */) && !forceCanceled)
		    {
			    Network?.PlayerDigging(DiggingStatus.Finished, _destroyingTarget, _destroyingFace);
			    Log.Debug($"Stopped breaking block. Ticks: {ticks}");

				Level.SetBlockState(_destroyingTarget, new Air().GetDefaultState());
            }
		    else
		    {
			    Network?.PlayerDigging(DiggingStatus.Cancelled, _destroyingTarget, _destroyingFace);
			    Log.Debug($"Cancelled breaking block. Ticks: {ticks}");
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

	    private bool HandleRightClick(SlotData slot, int hand)
	    {
            if (ItemFactory.ResolveItemName(slot.ItemID, out string itemName))
            {
	            IBlockState blockState = null;
	            if (ItemFactory.TryGetItem(itemName, out Item i))
	            {
		            if (i is ItemBlock ib)
		            {
			            blockState = ib.Block;
		            }
	            }

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

	                Log.Debug($"Placed block: {itemName} on {raytraceFloored} face= {face} facepos={remainder} ({adj})");

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