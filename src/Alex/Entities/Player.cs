using Alex.API.Graphics;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Gamestates.Playing;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Entities
{
    public class Player : PlayerMob
	{
        public static readonly float EyeLevel = 1.625F;

		public PlayerIndex PlayerIndex { get; }

		public bool CanFly { get; set; } = false;
		public bool IsFlying { get; set; } = false;

		public float FOVModifier { get; set; } = 0;

		public PlayerController Controller { get; }
		public Player(GraphicsDevice graphics, Alex alex, string name, World world, Texture2D skin, INetworkProvider networkProvider, PlayerIndex playerIndex) : base(name, world, networkProvider, skin, true)
	    {
		    PlayerIndex = playerIndex;
		    Controller = new PlayerController(graphics, world, alex.GameSettings, alex.InputManager, this, playerIndex); 
		    NoAi = false;

			Inventory = new Inventory(45);
	    }

		public override void Update(IUpdateArgs args)
		{
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

			base.Update(args);

		}

		public override void TerrainCollision(Vector3 collisionPoint, Vector3 direction)
		{
			base.TerrainCollision(collisionPoint, direction);
		}

		public override void OnTick()
		{
			base.OnTick();
		}
	}
}