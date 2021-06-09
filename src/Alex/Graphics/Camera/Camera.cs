using System;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using MathF = System.MathF;

namespace Alex.Graphics.Camera
{
    public class Camera : ICamera
    {
	    protected BoundingFrustum Frustum = new BoundingFrustum(Matrix.Identity);
	    public    BoundingFrustum BoundingFrustum => Frustum;// new BoundingFrustum(ViewMatrix * ProjectionMatrix);

	    /// <summary>
	    /// The nearest distance the camera will use
	    /// </summary>
	    public const float NearDistance = 0.015f;

	    /// <summary>
	    /// The furthest the camera can see
	    /// </summary>
	    public float FarDistance { get; set; }

	    public float FOV         { get; set; } = 75f;

	    public float FOVModifier
	    {
		    get => _fovModifier;
		    set
		    {
			    _fovModifier = value;
			    UpdateProjectionMatrix();
		    }
	    }

	    public Camera()
		{
			SetRenderDistance(12);
		}

		public void SetRenderDistance(int renderDistance)
		{
			FarDistance = renderDistance * 16;// MathF.Pow(renderDistance, 2f);
			
			ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(FOV + FOVModifier),
				AspectRatio,
				NearDistance,
				FarDistance);
		}
		
		public Vector3 Offset { get; private set; } = Vector3.Zero;

		private Matrix _projectionMatrix;
        /// <summary>
        /// 
        /// </summary>
        public Matrix ProjectionMatrix 
        {
	        get
	        {
		        return _projectionMatrix;
	        }
	        set
	        {
		        _projectionMatrix = value;
		      //  _frustum = new BoundingFrustum(_viewMatrix * value);
	        }
        }

        private Matrix _viewMatrix;

        /// <summary>
        /// 
        /// </summary>
        public Matrix ViewMatrix
        {
	        get
	        {
		        return _viewMatrix;
	        }
	        set
	        {
		        _viewMatrix = value;
		       // _frustum = new BoundingFrustum(value * _projectionMatrix);
	        }
        }

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

        public void UpdateOffset(Vector3 offset)
        {
	        Offset = offset;
	        UpdateViewMatrix();
        }
        
        public  Vector3 Direction;
        private float   _fovModifier = 0f;

        /// <summary>
        /// Updates the camera's looking vector.
        /// </summary>
        protected virtual void UpdateViewMatrix()
        {
	        Matrix rotationMatrix = MatrixHelper.CreateRotation(Rotation); 

	        Vector3 lookAtOffset = Vector3.Backward.Transform(rotationMatrix);
	        Direction = lookAtOffset;

	        var pos = Position;
	        
			Target = pos + lookAtOffset;
	        _viewMatrix = Matrix.CreateLookAt(pos, Target, Vector3.Up);
	        
	        Frustum = new BoundingFrustum(_viewMatrix * _projectionMatrix);
		}

	    public virtual void UpdateAspectRatio(float aspectRatio)
	    {
		    AspectRatio = aspectRatio;
		    UpdateProjectionMatrix();
	    }

	    private float AspectRatio { get; set; } = 0;

	    float ICamera.AspectRatio => this.AspectRatio;

	    public virtual void UpdateProjectionMatrix()
		{
			_projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(FOV + FOVModifier),
				AspectRatio,
				NearDistance,
				FarDistance);
			
			
			Frustum = new BoundingFrustum(_viewMatrix * _projectionMatrix);
		}

	    public virtual void MoveTo(Vector3 position, Vector3 rotation)
	    {
		    Position = position;
		    Rotation = rotation;
	    }

	    public virtual void Update(IUpdateArgs args)
		{
			//Update(args, entity.KnownPosition);
		}
    }
}