using Microsoft.Xna.Framework;

namespace Alex.Rendering.Camera
{
    public class FirstPersonCamera : Camera
    {
        /// <summary>
        /// The nearest distance the camera will use
        /// </summary>
        public const float NearDistance = 0.1f;

        /// <summary>
        /// The furthest the camera can see
        /// </summary>
        public float FarDistance { get; set; }

        public FirstPersonCamera(int renderDistance, Vector3 pos, Vector3 rot)
        {
            Position = pos;
            Rotation = rot;

            FarDistance = renderDistance * 16 * 16;

            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(70),
                1.333333F,
                NearDistance,
                FarDistance);
        }

        public void UpdateAspectRatio(float aspectRatio)
        {
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(70),
                aspectRatio,
                NearDistance,
                FarDistance);
        }

        public void MoveTo(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
		}

        public Vector3 PreviewMove(Vector3 amount)
        {
			Matrix rotate = Matrix.CreateRotationY(Rotation.Y);
            Vector3 movement = new Vector3(amount.X, amount.Y, amount.Z);
            movement = Vector3.Transform(movement, rotate);
            return Position + movement;
        }

        public void Move(Vector3 scale)
        {
            MoveTo(PreviewMove(scale), Rotation);
        }
    }
}