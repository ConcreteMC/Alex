using Microsoft.Xna.Framework;

namespace Alex.Rendering.Camera
{
    public class FirstPersonCamera : Camera
    {
        /// <summary>
        /// The nearest distance the camera will use
        /// </summary>
        public const float NearDistance = 0.05f;

        /// <summary>
        /// The furthest the camera can see
        /// </summary>
        public float FarDistance { get; set; }
        public float[] Distances = { 10f, 15f, 30f, 50f, 100f };

        public FirstPersonCamera(Vector3 pos, Vector3 rot)
        {
            Position = pos;
            Rotation = rot;

            FarDistance = (int)Game.RenderDistance;

            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                Game.GraphicsDevice.Viewport.AspectRatio,
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