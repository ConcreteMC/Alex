using System;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Hostile
{
	public class Zombie : HostileMob
	{
		public Zombie(World level) : base((EntityType)32, level)
		{
			Height = 1.95;
			Width = 0.6;
		}
		
		protected override void Animate(float distSQ, float dt, float mvSpeed)
		{
			if (_leftArmModel != null && _rightArmModel != null)
			{
				Vector3 rArmRot;

				if (mvSpeed > 0f)
				{
					rArmRot = new Vector3(85f, 0, 0);
				}
				else
				{
					IsMoving = false;

					rArmRot = new Vector3(
						85f, 0f, 0f);
				}


				if (!_leftArmModel.IsAnimating)
				{
					_leftArmModel.Rotation = rArmRot;
				}

				if (!_rightArmModel.IsAnimating)
				{
					_rightArmModel.Rotation = rArmRot;
				}
			}
		}
	}
}
