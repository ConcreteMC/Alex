using System;
using System.Diagnostics;
using Alex.API.Graphics;
using Alex.Graphics.Models.Entity;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Hostile
{
	public class Spider : HostileMob
	{
		/// <inheritdoc />
		public override bool IsWallClimbing
		{
			get
			{
				return base.IsWallClimbing;
			}
			set
			{
				base.IsWallClimbing = value;
				//ModelRenderer.Scale
			}
		}

		private EntityModelRenderer.ModelBone Leg0;
		private EntityModelRenderer.ModelBone Leg1;
		private EntityModelRenderer.ModelBone Leg2;
		private EntityModelRenderer.ModelBone Leg3;
		private EntityModelRenderer.ModelBone Leg4;
		private EntityModelRenderer.ModelBone Leg5;
		private EntityModelRenderer.ModelBone Leg6;
		private EntityModelRenderer.ModelBone Leg7;
		
		public Spider(World level) : base((EntityType)35, level)
		{
			JavaEntityId = 52;
			Height = 0.9;
			Width = 1.4;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 15 && entry is MetadataByte mtd)
			{
				IsWallClimbing = (mtd.Value & 0x01) != 0;
			}
		}

		/// <inheritdoc />
		protected override void UpdateModelParts()
		{
			if (ModelRenderer == null)
				return;

			var renderer = ModelRenderer;
			
			renderer.GetBone("leg0", out Leg0);
			renderer.GetBone("leg1", out Leg1);
			renderer.GetBone("leg2", out Leg2);
			renderer.GetBone("leg3", out Leg3);
			renderer.GetBone("leg4", out Leg4);
			renderer.GetBone("leg5", out Leg5);
			renderer.GetBone("leg6", out Leg6);
			renderer.GetBone("leg7", out Leg7);
		}

		private bool _wasMoving = false;
		private Stopwatch _timeSinceStartMoving = new Stopwatch();
		/// <inheritdoc />
		protected override void Animate(float distSQ, float dt)
		{
			bool moving = distSQ > 0f;

			if (moving != _wasMoving)
			{
				if (!_wasMoving && moving)
				{
					//_timeSinceStartMoving.Restart();
				}

				_wasMoving = moving;
			}
			
			if (!moving)
			{
				Leg0.Rotation = new Vector3(0f, 45f, -45f);
				Leg1.Rotation = new Vector3(0f, -45f, 45f);
				Leg2.Rotation = new Vector3(0f, 22.5f, -33.3f);
				Leg3.Rotation = new Vector3(0f, -22.5f, 33.3f);
				Leg4.Rotation = new Vector3(0f, -22.5f, -33.3f);
				Leg5.Rotation = new Vector3(0f, 22.5f, 33.3f);
				Leg6.Rotation = new Vector3(0f, -45f, -45f);
				Leg7.Rotation = new Vector3(0f, 45f, 45f);
				
				return;
			}

			if (!_timeSinceStartMoving.IsRunning)
			{
				_timeSinceStartMoving.Start();
			}

			var animTime = (float)_timeSinceStartMoving.Elapsed.TotalSeconds;
			
			Leg0.Rotation = new Vector3(0f, -MathF.Abs(MathF.Cos(animTime * 76.34f + 90f * 0f) * 22.92f), MathF.Abs(MathF.Sin(animTime * 38.17f + 90f * 0f) * 22.92f));
			Leg1.Rotation = new Vector3(0f, MathF.Abs(MathF.Cos(animTime * 76.34f + 90f * 0f) * 22.92f), -MathF.Abs(MathF.Sin(animTime * 38.17f + 90f * 0f) * 22.92f));
			Leg2.Rotation = new Vector3(0f, -MathF.Abs(MathF.Cos(animTime * 76.34f + 90f * 1f) * 22.92f), MathF.Abs(MathF.Sin(animTime * 38.17f + 90f * 1f) * 22.92f));
			Leg3.Rotation = new Vector3(0f, MathF.Abs(MathF.Cos(animTime * 76.34f + 90f * 1f) * 22.92f), -MathF.Abs(MathF.Sin(animTime * 38.17f + 90f * 1f) * 22.92f));
			Leg4.Rotation = new Vector3(0f, -MathF.Abs(MathF.Cos(animTime * 76.34f + 90f * 2f) * 22.92f), MathF.Abs(MathF.Sin(animTime * 38.17f + 90f * 2f) * 22.92f));
			Leg5.Rotation = new Vector3(0f, MathF.Abs(MathF.Cos(animTime * 76.34f + 90f * 2f) * 22.92f), -MathF.Abs(MathF.Sin(animTime * 38.17f + 90f * 2f) * 22.92f));
			Leg6.Rotation = new Vector3(0f, -MathF.Abs(MathF.Cos(animTime * 76.34f + 90f * 3f) * 22.92f), MathF.Abs(MathF.Sin(animTime * 38.17f + 90f * 3f) * 22.92f));
			Leg7.Rotation = new Vector3(0f, MathF.Abs(MathF.Cos(animTime * 76.34f + 90f * 3f) * 22.92f), -MathF.Abs(MathF.Sin(animTime * 38.17f + 90f * 3f) * 22.92f));
		}
	}
}
