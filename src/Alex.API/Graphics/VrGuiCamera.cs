using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
    public class VrGuiCamera : ICamera
    {
        public BoundingFrustum BoundingFrustum { get; }
        public float FarDistance { get; set; } = 10000f;
        public float NearDistance { get; set; } = 0.15f;
        public float FOV { get; set; } = 1f;
        public float AspectRatio { get; } = (float) Math.PI / 4f;
        public Vector3 Position { get; set; } = new Vector3(0, 0, 100);
        public Matrix ProjectionMatrix { get; set; } = Matrix.Identity;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Target { get; } = Vector3.Zero;
        public Vector3 Up { get; set; } = Vector3.Up;
        public Vector3 Forward { get; set; } = Vector3.Forward;
        public Matrix ViewMatrix { get; set; } = Matrix.Identity;
        
        public void Move(Vector3 scale)
        {
            
        }

        public void MoveTo(Vector3 position, Vector3 rotation)
        {
            
        }

        public Vector3 PreviewMove(Vector3 amount)
        {
            return Position;
        }

        public void Update(IUpdateArgs args)
        {
            
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