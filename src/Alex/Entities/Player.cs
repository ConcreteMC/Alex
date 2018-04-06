using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Gamestates.Playing;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Worlds;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Entities
{
    public class Player : PlayerMob
	{
        public static readonly float EyeLevel = 1.625F;

		public bool CanFly { get; set; } = false;
		public bool IsFlying { get; set; } = false;
		public Gamemode Gamemode { get; set; }

		public float FOVModifier { get; set; } = 0;

		public PlayerController Controller { get; }
		public Player(GraphicsDevice graphics, Alex alex, string name, World world, Texture2D skin) : base(name, world)
	    {
		    Controller = new PlayerController(graphics, world, alex.GameSettings, this); 

			if (Alex.Instance.Resources.BedrockResourcePack.EntityModels.TryGetValue("geometry.humanoid.customSlim",
			    out EntityModel m))
		    {
				ModelRenderer = new EntityModelRenderer(m, skin);
		    }
	    }

		public override void Update(IUpdateArgs args)
		{
			if (Controller.IsFreeCam && !CanFly)
			{
				Controller.IsFreeCam = false;
			}
			else if (CanFly)
			{
				IsFlying = Controller.IsFreeCam;
			}
			
			Controller.Update(args.GameTime);
			base.Update(args);

		}
	}
}