using System;
using System.Diagnostics;
using Alex.Graphics.Models.Entity;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Spider : HostileMob
	{
		public Spider(World level) : base(level)
		{
			Height = 0.9;
			Width = 1.4;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataByte mtd)
			{
				IsWallClimbing = (mtd.Value & 0x01) != 0;
			}
		}

		private bool _wasMoving = false;

		private Stopwatch _timeSinceStartMoving = new Stopwatch();
		/// <inheritdoc />
		/*protected override void Animate(float dt, float mvSpeed)
		{
			bool moving = mvSpeed > 0f;

			if (moving != _wasMoving)
			{
				if (!_wasMoving && moving)
				{
					_timeSinceStartMoving.Restart();
				}

				_wasMoving = moving;
			}
			
			if (!moving)
			{
				Leg0.Rotation = new Vector3(0f, 45f, 45f);
				Leg1.Rotation = new Vector3(0f, -45f, -45f);
				Leg2.Rotation = new Vector3(0f, 22.5f, 33.3f);
				Leg3.Rotation = new Vector3(0f, -22.5f, -33.3f);
				Leg4.Rotation = new Vector3(0f, -22.5f, 33.3f);
				Leg5.Rotation = new Vector3(0f, 22.5f, -33.3f);
				Leg6.Rotation = new Vector3(0f, -45f, 45f);
				Leg7.Rotation = new Vector3(0f, 45f, -45f);
				
				return;
			}

			if (!_timeSinceStartMoving.IsRunning)
			{
				_timeSinceStartMoving.Start();
			}

			var animTime = (float)_timeSinceStartMoving.Elapsed.TotalSeconds;
			
			Leg0.Rotation = new Vector3(0f, -MathF.Abs(MathF.Cos(animTime * 76.34f* 0f) * 22.92f), MathF.Abs(MathF.Sin(animTime * 38.17f * 0f) * 22.92f));
			Leg1.Rotation = new Vector3(0f, MathF.Abs(MathF.Cos(animTime * 76.34f * 0f) * 22.92f), -MathF.Abs(MathF.Sin(animTime * 38.17f * 0f) * 22.92f));
			Leg2.Rotation = new Vector3(0f, -MathF.Abs(MathF.Cos(animTime * 76.34f * 1f) * 22.92f), MathF.Abs(MathF.Sin(animTime * 38.17f * 1f) * 22.92f));
			Leg3.Rotation = new Vector3(0f, MathF.Abs(MathF.Cos(animTime * 76.34f* 1f) * 22.92f), -MathF.Abs(MathF.Sin(animTime * 38.17f * 1f) * 22.92f));
			Leg4.Rotation = new Vector3(0f, -MathF.Abs(MathF.Cos(animTime * 76.34f * 2f) * 22.92f), MathF.Abs(MathF.Sin(animTime * 38.17f * 2f) * 22.92f));
			Leg5.Rotation = new Vector3(0f, MathF.Abs(MathF.Cos(animTime * 76.34f * 2f) * 22.92f), -MathF.Abs(MathF.Sin(animTime * 38.17f * 2f) * 22.92f));
			Leg6.Rotation = new Vector3(0f, -MathF.Abs(MathF.Cos(animTime * 76.34f * 3f) * 22.92f), MathF.Abs(MathF.Sin(animTime * 38.17f * 3f) * 22.92f));
			Leg7.Rotation = new Vector3(0f, MathF.Abs(MathF.Cos(animTime * 76.34f * 3f) * 22.92f), -MathF.Abs(MathF.Sin(animTime * 38.17f * 3f) * 22.92f));
		}*/
	}
}