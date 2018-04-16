using System;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.Entities;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Rendering.Camera
{
    public class Camera : ICamera
	{
        public BoundingFrustum BoundingFrustum => new BoundingFrustum(ViewMatrix * ProjectionMatrix);

	    /// <summary>
	    /// The nearest distance the camera will use
	    /// </summary>
	    public const float NearDistance = 0.1f;

	    /// <summary>
	    /// The furthest the camera can see
	    /// </summary>
	    public float FarDistance { get; set; }

	    public float FOV { get; set; } = 75;
		public Camera(int renderDistance)
	    {
		    FarDistance = renderDistance * 16 * 16;


		    ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
			    MathHelper.ToRadians(FOV),
			    1.333333F,
			    NearDistance,
			    FarDistance);
		}

        /// <summary>
        /// 
        /// </summary>
        public Matrix ProjectionMatrix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Matrix ViewMatrix { get; set; }

	    /// <summary>
        /// 
        /// </summary>
        public Vector3 Target { get; protected set; }
        private Vector3 _position;
        /// <summary>
        /// Our current position.
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
	            UpdateViewMatrix();
            }
        }

        private Vector3 _rotation;
        /// <summary>
        /// Our current rotation
        /// </summary>
        public Vector3 Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
	            UpdateViewMatrix();
            }
        }

        public Vector3 Direction;
        /// <summary>
        /// Updates the camera's looking vector.
        /// </summary>
        protected virtual void UpdateViewMatrix()
        {
			Matrix rotationMatrix = Matrix.CreateRotationX(Rotation.Z) * //Pitch
			                        Matrix.CreateRotationY(Rotation.Y); //Yaw

	        Vector3 lookAtOffset = Vector3.Transform(Vector3.UnitZ, rotationMatrix);

	        Target = Position + lookAtOffset;

	        Direction = Vector3.Transform(Vector3.Forward, rotationMatrix);

	        ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up);
		}

	    public virtual void UpdateAspectRatio(float aspectRatio)
	    {
		    _aspectRatio = aspectRatio;
		    UpdateProjectionMatrix();
	    }

		private float _aspectRatio = 0;
		public void UpdateProjectionMatrix()
		{
			ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(FOV),
				_aspectRatio,
				NearDistance,
				FarDistance);
		}

	    public virtual void MoveTo(Vector3 position, Vector3 rotation)
	    {
		    Position = position;
		    Rotation = rotation;
	    }

	    public virtual Vector3 PreviewMove(Vector3 amount)
	    {
		    Matrix rotate = Matrix.CreateRotationY(Rotation.Y);
		    Vector3 movement = new Vector3(amount.X, amount.Y, amount.Z);
		    movement = Vector3.Transform(movement, rotate);
		    return Position + movement;
	    }

	    public virtual void Move(Vector3 scale)
	    {
		    MoveTo(PreviewMove(scale), Rotation);
	    }

	    public virtual void Update(IUpdateArgs args, IEntity entity)
	    {
			
			MoveTo(entity.KnownPosition, 
				new Vector3(MathHelper.ToRadians(entity.KnownPosition.HeadYaw), MathHelper.ToRadians(entity.KnownPosition.Yaw), MathHelper.ToRadians(entity.KnownPosition.Pitch)));
	    }
	}
}