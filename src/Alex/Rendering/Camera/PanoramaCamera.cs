using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.Networking.Java;
using Microsoft.Xna.Framework;

namespace Alex.Rendering.Camera
{
    public class PanoramaCamera : ICamera
    {
        private Vector3 _position;
        private Vector3 _rotation;

        
        public BoundingFrustum BoundingFrustum => new BoundingFrustum(ViewMatrix * ProjectionMatrix);

        public float FarDistance { get; set; }

        public float FOV { get; set; }

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdateViewMatrix();
            }
        }

        public Matrix ProjectionMatrix { get; set; }

        public Vector3 Rotation
        {
            get => _rotation;
            set { 
                _rotation = value;
                UpdateViewMatrix();
            }
        }

        public Vector3 Target { get; set; }

        public Matrix ViewMatrix { get; set; }
        
        public float AspectRatio {get; private set; }

        public PanoramaCamera(Vector3 position, Vector3 rotation)
        {
            FarDistance = 512.0f;

            FOV = 45.0f;
            
            Position = position;
            Rotation = rotation;

            UpdateAspectRatio(1.333333F);
        }

        public void Move(Vector3 scale)
        {
            throw new NotImplementedException();
        }

        public void MoveTo(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 PreviewMove(Vector3 amount)
        {
            throw new NotImplementedException();
        }

        public void Update(IUpdateArgs args, IEntity entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateViewMatrix()
        {
            var rotationMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);

            var up = Vector3.Transform(Vector3.Up, rotationMatrix);

            var direction = Vector3.Transform(Vector3.Forward, rotationMatrix);
            
            Target = Position + direction;
            ViewMatrix = Matrix.CreateLookAt(Position, Target, up);
        }

        public void UpdateAspectRatio(float aspectRatio)
        {
            AspectRatio = aspectRatio;
            UpdateProjectionMatrix();
        }

        public void UpdateProjectionMatrix()
        {
            ProjectionMatrix =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), AspectRatio, 0.2f, FarDistance);
        }
    }
}
