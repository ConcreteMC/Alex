using Microsoft.Xna.Framework;

namespace Alex.Graphics.Camera
{
    public class ThirdPersonCamera : Camera
    {
		public ThirdPersonCamera(int renderDistance, Vector3 positiuon, Vector3 rotation) : base(renderDistance)
		{
			Position = positiuon;
			Rotation = rotation;
		}


		private Vector3 _thirdPersonOffset =  new Vector3(0, 2.5f, -3.5f);
	    protected override void UpdateViewMatrix()
	    {
            
		    Matrix rotationMatrix = Matrix.CreateRotationX(-Rotation.Z) * //Pitch
		                            Matrix.CreateRotationY(-Rotation.Y); //Yaw

			Vector3 lookAtOffset = Vector3.Transform(_thirdPersonOffset, rotationMatrix);

		    Target = Position;

		    Direction = Vector3.Transform(Vector3.Backward, rotationMatrix);


		    var heightOffset = new Vector3(0, 1.8f, 0);
			ViewMatrix = Matrix.CreateLookAt(Position + lookAtOffset, Target + heightOffset, Vector3.Up);
        }
    }
}
