using System;
using System.Collections.Generic;
using Alex.Net;
using Alex.Particles;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using MiNET.Particles;

namespace Alex.Entities.Projectiles
{
	public class FireworkRocket : ThrowableEntity
	{
		/// <inheritdoc />
		public FireworkRocket(World level) : base(level)
		{
			Width = 0.25;
			Height = 0.25;
			
			Gravity = 0.0;
			Drag = 0.01;
			
			HasCollision = false;
			IsAffectedByGravity = true;
			StopOnImpact = true;
		}

		/// <inheritdoc />
		public override bool NoAi => false;

		/// <inheritdoc />
		protected override void OnModelUpdated()
		{
			base.OnModelUpdated();
			var model = ModelRenderer?.Model;

			if (model != null)
			{
				model.Root.BaseRotation = new Vector3(-90f, 0f, 0f);
			}
		}

		/// <inheritdoc />
		public override void HandleEntityEvent(byte eventId, int data)
		{
			if (eventId == 25)
			{ 
				//Explode();
				return;
			}
			base.HandleEntityEvent(eventId, data);
		}

		private bool _exploded = false;
		private void Explode()
		{
			_exploded = true;
			
			var center = KnownPosition.ToVector3();
			foreach (var position in GetParticlePositions(10f))
			{
				Alex.Instance?.ParticleManager?.SpawnParticle(
					"redstone_wire_dust_particle", center + position, out var particleInstance, 0l,
					ParticleDataMode.Color);
			}
		}

		private IEnumerable<Vector3> GetParticlePositions(float radius)
		{
			for (var degree = 0f; degree < 1f; degree += 0.1f)
			{
				yield return new Vector3(MathF.Sin(degree) * radius, MathF.Cos((degree * 2f) - 1f) * radius, MathF.Cos(degree) * radius);
			}
		}

		/// <inheritdoc />
		public override float CollidedWithWorld(Vector3 direction, Vector3 position, float impactVelocity)
		{
			if (StopOnImpact)
			{
				if (direction == Vector3.Up)
				{
					Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);

					return 0f;
				}
			}

			return 0f;
		}

		/// <inheritdoc />
		public override void OnTick()
		{
			base.OnTick();

			if (Age % 10 == 0 && !_exploded)
			{
				Alex.Instance?.ParticleManager?.SpawnParticle(ParticleType.FireworksSpark, RenderLocation.ToVector3());
			}
		}
	}
}