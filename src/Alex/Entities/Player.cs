using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Worlds;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Entities
{
    public class Player : PlayerMob
	{
        public static readonly float EyeLevel = 1.625F;

	    public Player(string name, World world, Texture2D skin) : base(name, world)
	    {
		    if (Alex.Instance.Resources.BedrockResourcePack.EntityModels.TryGetValue("geometry.humanoid.customSlim",
			    out EntityModel m))
		    {
				ModelRenderer = new EntityModelRenderer(m, skin);
		    }
	    }
    }
}