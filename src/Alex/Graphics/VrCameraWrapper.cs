using Microsoft.Xna.Framework;
using SharpVR;

namespace Alex.API.Graphics
{
    public class VrCameraWrapper : ICameraWrapper
    {
        private readonly VrContext _vrContext;
        private Eye _eye;
        private Matrix _hmd;
        internal VrCameraWrapper(VrContext vrContext)
        {
            _vrContext = vrContext;
        }

        internal void Update(Eye eye, Matrix hmd)
        {
            _eye = eye;
            _hmd = hmd;
        }
        
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Forward { get; private set; }
        public Vector3 Up { get; private set; }

        public void PreDraw(ICamera camera)
        {
            Matrix projection;
            var eyeMatrix = Matrix.Identity;
            _vrContext.GetProjectionMatrix(_eye, camera.NearDistance, camera.FarDistance, out var hmdProjection);
            _vrContext.GetEyeMatrix(_eye, out var hmdEye);
            projection = hmdProjection.ToMg();
            eyeMatrix = hmdEye.ToMg();

            var forward = Vector3.TransformNormal(camera.Forward, Matrix.Invert(_hmd * eyeMatrix));
            var target = camera.Position - forward;
            var view = Matrix.CreateLookAt(camera.Position, camera.Target, camera.Up);

            //View = view * Matrix.Invert(_hmd * eyeMatrix);
            View = view * _hmd * eyeMatrix;
            Projection = projection;
            Forward = forward;
            Up = camera.Up;
            Position = camera.Position;
            camera.ProjectionMatrix = Projection;
            camera.ViewMatrix = View;
        }
    }
}