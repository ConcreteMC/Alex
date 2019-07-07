using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Camera
{
    public class FirstPersonCamera : Camera
    {
        public FirstPersonCamera(int renderDistance, Vector3 pos, Vector3 rot) : base(renderDistance)
        {
            Position = pos;
            Rotation = rot;
        }

	    public override void Update(IUpdateArgs args, IEntity entity)
	    {
		    MoveTo(entity.KnownPosition + new Vector3(0, Player.EyeLevel, 0),
			    new Vector3(MathHelper.ToRadians(entity.KnownPosition.HeadYaw), MathHelper.ToRadians(entity.KnownPosition.HeadYaw), MathHelper.ToRadians(entity.KnownPosition.Pitch)));
	    }
	}
}