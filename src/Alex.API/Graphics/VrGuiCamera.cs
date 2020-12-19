using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using NLog;
using SharpVR;

namespace Alex.API.Graphics
{
    public class VrGuiCamera : ICamera
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();
        public BoundingFrustum BoundingFrustum { get; }
        public float FarDistance { get; set; } = 1000f;
        public float NearDistance { get; set; } = 0.15f;
        public float FOV { get; set; } = 1f;
        public float AspectRatio { get; } = 1f;
        public Vector3 Position
        {
            get => _basePosition + _vrOffsetPosition;
            set => _basePosition = value;
        }
        public Matrix ProjectionMatrix { get; set; } = Matrix.Identity;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Target { get; private set; } = Vector3.Zero;
        public Vector3 Up { get; set; } = Vector3.UnitZ;
        public Vector3 Forward { get; set; } = Vector3.Backward;
        public Matrix ViewMatrix { get; set; } = Matrix.Identity;

        private Vector3 _basePosition = new Vector3(0f, 10f, 10f), _vrOffsetPosition;
        
        
        public void Move(Vector3 scale)
        {
            
        }

        public void MoveTo(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 PreviewMove(Vector3 amount)
        {
            return Position;
        }

        public void Update(IUpdateArgs args)
        {
            var cxt = VrContext.Get();
            if (!(cxt.Initialized && cxt.Hmd.IsConnected)) return;
            
            //cxt.Hmd.GetPose().Decompose(out var scale, out var rotation, out var translation);

            //_vrOffsetPosition = translation;
            //Target = Position + Vector3.Transform(Vector3.Backward, rotation);
            Forward = Vector3.Normalize(Target - Position);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(FOV, AspectRatio, 0.1f, 1000.0f);
            //ViewMatrix = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(Position);
            ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.UnitZ);
            // var scaleStr = $"Scale: {scale.X:F2}, {scale.Y:F2}, {scale.Z:F2}";
            // var rotationStr = $"Rotation: {rotation.X:F2}, {rotation.Y:F2}, {rotation.Z:F2}";
            // var translationStr = $"Translation: {translation.X:F2}, {translation.Y:F2}, {translation.Z:F2}";
            // var targetStr = $"Target: {Target.X:F2}, {Target.Y:F2}, {Target.Z:F2}";
            // var forwardStr = $"Forward: {Forward.X:F2}, {Forward.Y:F2}, {Forward.Z:F2}";
            // Log.Debug($"HMD POSE: {scaleStr:48} | {rotationStr:48} | {translationStr:48} | {targetStr:48} | {forwardStr:48}");
        }

        public void UpdateAspectRatio(float aspectRatio)
        {
            
        }

        public void UpdateProjectionMatrix()
        {
            
        }

        public void SetRenderDistance(int renderDistance)
        {
            
        }
    }
}