using System;
using Alex.API.Data;
using Alex.API.Graphics;
using Alex.API.Input;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.GameStates.Playing;
using Alex.Graphics.Models.Entity;
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

	    private bool _destroyingBlock = false;
        private int _destroyingTick = 0;
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
		    _destroyingBlock = true;
		    Log.Debug($"Start break block ({Raytraced.Floor()})");
        }

	    private void StopBreakingBlock()
	    {
		    Log.Debug($"Stopped breaking block. Ticks: {_destroyingTick}");

		    _destroyingBlock = false;
            _destroyingTick = 0;
        }

	    private bool HandleRightClick(SlotData slot, int hand)
	    {
            if (ItemFactory.ResolveItemName(slot.ItemID, out string itemName))
            {
                var blockState = BlockFactory.GetBlockState(itemName);

                if (blockState != null && !(blockState.Block is Air) && HasRaytraceResult)
                {
                    var flooredAdj = AdjacentRaytrace.Floor();

                    var adj = AdjacentRaytrace.Floor() - Raytraced.Floor();
                    adj.Normalize();

	                var face = adj.GetBlockFace();

                    var remainder = new Vector3(AdjacentRaytrace.X - flooredAdj.X, AdjacentRaytrace.Y - flooredAdj.Y, AdjacentRaytrace.Z - flooredAdj.Z);

                    var r = Raytraced.Floor();
                    Network.BlockPlaced(new BlockCoordinates((int)r.X, (int)r.Y, (int)r.Z), face, hand, remainder);

	                var coordR = new BlockCoordinates(r);

                    var existingBlock = Level.GetBlock(coordR);
	                if (existingBlock.IsReplacible || !existingBlock.Solid)
	                {
		                Level.SetBlockState(coordR, blockState);
                    }
	                else
	                {
		                Level.SetBlockState(new BlockCoordinates(r + adj), blockState);
	                }

	                Log.Debug($"Placed block: {itemName} on {r} face= {face} facepos={remainder} ({adj})");

	                return true;
                }
                else if (blockState == null)
                {
                    Network.UseItem(hand);
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