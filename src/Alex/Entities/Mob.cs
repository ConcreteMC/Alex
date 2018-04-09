using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	public class Mob : Entity
	{
		public Mob(int entityTypeId, World level) : base(entityTypeId, level)
		{
			Width = Length = 0.6;
			Height = 1.80;
		}

		public Mob(EntityType mobTypes, World level) : this((int)mobTypes, level)
		{
		}

		public override void OnTick()
		{
			base.OnTick();

			if (Velocity.Length() > 0)
			{
				PlayerLocation oldPosition = (PlayerLocation)KnownPosition.Clone();
				bool onGroundBefore = IsOnGround(KnownPosition);

				KnownPosition.X += (float)Velocity.X;
				KnownPosition.Y += (float)Velocity.Y;
				KnownPosition.Z += (float)Velocity.Z;

				bool onGround = IsOnGround(KnownPosition);
				if (!onGroundBefore && onGround)
				{
					KnownPosition.Y = (float)Math.Floor(oldPosition.Y);
					Velocity = Vector3.Zero;
				}
				else
				{
					Velocity *= (float)(1.0f - Drag);
				//	if (!onGround)
				//	{
				//		Velocity -= new Vector3(0, (float)Gravity, 0);
				//	}
				}

				KnownPosition.OnGround = onGround;
				LastUpdatedTime = DateTime.UtcNow;
			}
			else if (Velocity != Vector3.Zero)
			{
				Velocity = Vector3.Zero;
				LastUpdatedTime = DateTime.UtcNow;
			}
		}

		protected bool IsOnGround(PlayerLocation position)
		{
			PlayerLocation pos = (PlayerLocation)position.Clone();
			pos.Y -= 0.1f;
			IBlock block = Level.GetBlock(new BlockCoordinates(pos));

			return block.Solid;
		}

		protected bool IsInGround(PlayerLocation position)
		{
			PlayerLocation pos = (PlayerLocation)position.Clone();
			IBlock block = Level.GetBlock(new BlockCoordinates(pos));

			return block.Solid;
		}

	}
}
