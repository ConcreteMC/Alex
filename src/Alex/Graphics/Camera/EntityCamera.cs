using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
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

		private int   _cameraIndex = 0;
		public void ToggleMode()
		{
			Mode = _modes[(_cameraIndex++ % _modes.Length)];
		}

		protected override void UpdateViewMatrix()
		{
			switch (_mode)
			{
				case EntityCameraMode.FirstPerson:
					UpdateViewMatrixFirstPerson();
					break;
				case EntityCameraMode.ThirdPersonFront:
					UpdateThirdPerson(true);
					break;
				case EntityCameraMode.ThirdPersonBack:
					UpdateThirdPerson(false);
					break;
			}
		}

		private void UpdateViewMatrixFirstPerson()
		{
			var position = Position;

			if (TrackingEntity.IsSneaking)
			{
				position.Y -= 0.18f;
			}
			
			var direction = new Vector3(Rotation.X, Rotation.Y, Rotation.Z);
			direction.Normalize();
			
			//var target = position
			
			Target = position + direction;
			Direction = direction;

			ViewMatrix = Matrix.CreateLookAt(position, Target, Vector3.Up);
			Frustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);
		}
		
		private static readonly Vector3 ThirdPersonOffset =  new Vector3(0, 2.5f, 3.5f);

		private void UpdateThirdPerson(bool frontSideView)
		{
			//var direction = new Vector3(Rotation.X, Rotation.Y, Rotation.Z);
			//direction.Normalize();
			
			//var target = position
			
			//Target = position + direction;
			//Direction = direction;

			//var offset = ThirdPersonOffset;
			//offset = Vector3.Transform(offset, MatrixHelper.CreateRotationDegrees(Rotation));
			var boundingBox = TrackingEntity.GetBoundingBox();
			
			var renderLocation = TrackingEntity.RenderLocation;
			
			var target = Position;
			target.Y += boundingBox.GetHeight();
			
			float pitch = (-renderLocation.Pitch).ToRadians();
			float yaw   = ((renderLocation.Yaw)).ToRadians();

			var directionMatrix = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);
			
			Vector3 dir = frontSideView ? Vector3.Forward : Vector3.Backward;
			dir = Vector3.Transform(dir, directionMatrix);

			Direction = dir;

			var cameraPosition = new Vector3(target.X, target.Y, target.Z);
			cameraPosition += (dir * ThirdPersonOffset.Z);

			Target = target;
			ViewMatrix = Matrix.CreateLookAt(cameraPosition, target, Vector3.Up);
			Frustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);
		}

		public override void Update(IUpdateArgs args)
		{
			var entityLocation = TrackingEntity.RenderLocation.ToVector3();
			var entityPhysicalLocation = TrackingEntity.KnownPosition;
			
			if (_mode == EntityCameraMode.FirstPerson)
			{
				var boundingBox = TrackingEntity.GetBoundingBox();
				MoveTo(
					entityLocation + new Vector3(0, (float) (boundingBox.GetHeight() - 0.175f), 0),
						entityPhysicalLocation.GetDirection(true, true));
			}
			else
			{
				MoveTo(entityLocation, 
					entityPhysicalLocation.GetDirection(true, true));
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