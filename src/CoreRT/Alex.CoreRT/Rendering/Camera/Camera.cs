using System;
using Alex.CoreRT.Utils;
using Microsoft.Xna.Framework;

namespace Alex.CoreRT.Rendering.Camera
{
    public class Camera
    {
        public BoundingFrustum BoundingFrustum => new BoundingFrustum(ViewMatrix * ProjectionMatrix);

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
        public Vector3 Target { get; private set; }
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
				UpdateLookAt();
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
				UpdateLookAt();
            }
        }

	    public float Yaw
	    {
		    get
			{
			 	Vector3 v = Direction;
				return MathUtils.RadianToDegree((float)Math.Atan2(v.X, v.Z));
			}
	    }

	    public float Pitch;

        public Vector3 Direction;
        /// <summary>
        /// Updates the camera's looking vector.
        /// </summary>
        protected void UpdateLookAt()
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(Rotation.X) *
                                  Matrix.CreateRotationY(Rotation.Y);

            Vector3 lookAtOffset = Vector3.Transform(Vector3.UnitZ, rotationMatrix);

            Target = Position + lookAtOffset;

            Direction = Vector3.Transform(Vector3.Forward, rotationMatrix);

			ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up);
		}
    }
}