using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Camera
{
	public class EntityCamera : Camera
	{
		private static readonly EntityCameraMode[] _modes = new EntityCameraMode[]
		{
			EntityCameraMode.FirstPerson,
			EntityCameraMode.ThirdPersonFront,
			EntityCameraMode.ThirdPersonBack
		};
		
		public Entity TrackingEntity { get; }

		private EntityCameraMode _mode;

		public EntityCameraMode Mode
		{
			get
			{
				return _mode;
			}
			set
			{
				_mode = value;

				switch (value)
				{
					case EntityCameraMode.FirstPerson:
						TrackingEntity.RenderEntity = false;
						TrackingEntity.IsFirstPersonMode = true;
						break;
					case EntityCameraMode.ThirdPersonBack:
					case EntityCameraMode.ThirdPersonFront:
						TrackingEntity.RenderEntity = true;
						TrackingEntity.IsFirstPersonMode = false;
						break;
				}
			}
		}

		public EntityCamera(Entity entity)
		{
			TrackingEntity = entity;
			Mode = EntityCameraMode.FirstPerson;
		}

		private int _cameraIndex = 0;
		
		public void ToggleMode()
		{
			Mode = _modes[(_cameraIndex++ % _modes.Length)];
		}

		protected override void UpdateViewMatrix()
		{
			switch (_mode)
			{
				case EntityCameraMode.FirstPerson:
					base.UpdateViewMatrix();
					break;
				case EntityCameraMode.ThirdPersonFront:
					UpdateThirdPerson(true);
					break;
				case EntityCameraMode.ThirdPersonBack:
					UpdateThirdPerson(false);
					break;
			}
		}
		
		private static readonly Vector3 ThirdPersonOffset =  new Vector3(0, 2.5f, 3.5f);
		private void UpdateThirdPerson(bool frontSideView)
		{
			Matrix rotationMatrix = Matrix.CreateRotationX(Rotation.Z) * //Pitch
			                        Matrix.CreateRotationY(-Rotation.Y); //Yaw

			Vector3 lookAtOffset = Vector3.Transform(
				new Vector3(
					ThirdPersonOffset.X, ThirdPersonOffset.Y,
					frontSideView ? ThirdPersonOffset.Z : -ThirdPersonOffset.Z), rotationMatrix);

			Target = Position;

			Direction = Vector3.Transform(Vector3.Backward, rotationMatrix);


			var heightOffset = new Vector3(0, 1.8f, 0);
			ViewMatrix = Matrix.CreateLookAt(Position + lookAtOffset, Target + heightOffset, Vector3.Up);
		}

		public override void Update(IUpdateArgs args)
		{
			var entityLocation = TrackingEntity.RenderLocation;

			
			if (_mode == EntityCameraMode.FirstPerson)
			{
				MoveTo(
					entityLocation + new Vector3(0, Player.EyeLevel, 0),
					new Vector3(
						MathHelper.ToRadians(entityLocation.HeadYaw),
						MathHelper.ToRadians(entityLocation.HeadYaw),
						MathHelper.ToRadians(entityLocation.Pitch)));
			}
			else
			{
				MoveTo(entityLocation.ToVector3(), 
					new Vector3(MathHelper.ToRadians(entityLocation.HeadYaw), MathHelper.ToRadians(entityLocation.HeadYaw), MathHelper.ToRadians(entityLocation.Pitch)));
			}
		}
	}

	public enum EntityCameraMode
	{
		FirstPerson,
		ThirdPersonFront,
		ThirdPersonBack
	}
}