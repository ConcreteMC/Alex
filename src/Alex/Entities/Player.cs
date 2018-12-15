using System;
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

		    if (Controller.InputManager.IsDown(InputCommand.LeftClick)) //Destroying block.
		    {
                Log.Debug($"Left click!");
		        if (_destroyingBlock)
		        {
		            _destroyingTick++;
                }
		        else
		        {
		            _destroyingBlock = true;
		        }
		    }
            else if (_destroyingBlock && Controller.InputManager.IsUp(InputCommand.LeftClick))
		    {
		        _destroyingBlock = false;
		        _destroyingTick = 0;
		    }

		    if (Controller.InputManager.IsPressed(InputCommand.RightClick))
		    {
               // Log.Debug($"Right click!");
		        if (Inventory.MainHand != null && Inventory.MainHand.ItemID != -1 && Inventory.MainHand.ItemID != 0)
		        {
		            if (ItemFactory.ResolveItemName(Inventory.MainHand.ItemID, out string itemName))
		            {
			            var blockState = BlockFactory.GetBlockState(itemName);

                        if (blockState != null && !(blockState.Block is Air))
		                {
			                var flooredAdj = AdjacentRaytrace.Floor();

                            var adj = AdjacentRaytrace.Floor() - Raytraced.Floor();
		                    adj.Normalize();

		                    BlockFace face = BlockFace.None;

                            if (adj == Vector3.Up)
		                    {
		                        face = BlockFace.Up;
		                    }
                            else if (adj == Vector3.Down)
		                    {
		                        face = BlockFace.Down;
		                    }
                            else if (adj == Vector3.Backward)
		                    {
		                        face = BlockFace.South;
		                    }
                            else if (adj == Vector3.Forward)
		                    {
		                        face = BlockFace.North;
		                    }
                            else if (adj == Vector3.Left)
		                    {
		                        face = BlockFace.West;
		                    }
                            else if (adj == Vector3.Right)
		                    {
		                        face = BlockFace.East;
		                    }

							var remainder = new Vector3(AdjacentRaytrace.X - flooredAdj.X, AdjacentRaytrace.Y - flooredAdj.Y, AdjacentRaytrace.Z - flooredAdj.Z);

                            var r = Raytraced.Floor();
                            Network.BlockPlaced(new BlockCoordinates((int)r.X, (int)r.Y, (int)r.Z), face, 0, remainder);
                            Log.Debug($"Placed block: {itemName} on {r} face= {face} facepos={remainder} ({adj})");
                        }
		                else if (blockState == null)
		                {
							Network.UseItem(0);
							Log.Debug($"Used item!");
		                }
		            }
		        }
		    }
			
			base.Update(args);

		}

		public override void TerrainCollision(Vector3 collisionPoint, Vector3 direction)
		{
		//	Log.Debug($"Terrain collision: {collisionPoint.ToString()} | {direction}");	
			base.TerrainCollision(collisionPoint, direction);
		}

		public override void OnTick()
		{
			base.OnTick();
		}
	}
}