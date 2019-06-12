using Microsoft.Xna.Framework;

namespace Alex.Graphics.Camera
{
    public class TopDownCamera : Camera
    {
        public TopDownCamera(int renderDistance) : base(renderDistance)
        {
        }

        public override void UpdateProjectionMatrix()
        {
            ProjectionMatrix = Matrix.CreateOrthographic(256, 256, 0.05f, 256f);
        }

        protected override void UpdateViewMatrix()
        {
            var normalPosition = new Vector3(Position.X, 256, Position.Z);
            var targetPosition = new Vector3(normalPosition.X, 0, normalPosition.Z);

            ViewMatrix = Matrix.CreateLookAt(normalPosition, targetPosition, Vector3.Forward);
        }
    }
}
