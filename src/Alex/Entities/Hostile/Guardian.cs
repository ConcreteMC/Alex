using Alex.Worlds;
using Microsoft.Xna.Framework;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Guardian : HostileMob
	{
		public Guardian(World level) : base(level)
		{
			Height = 0.85;
			Width = 0.85;
		}

		public Vector2 EyeTargetRotation { get; set; } = Vector2.Zero;

		[MoLang.Attributes.MoProperty("eye_target_x_rotation")]
		public double EyeTargetXRotation => EyeTargetRotation.X;

		[MoLang.Attributes.MoProperty("eye_target_y_rotation")]
		public double EyeTargetYRotation => EyeTargetRotation.Y;
	}
}