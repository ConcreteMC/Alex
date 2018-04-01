using Microsoft.Xna.Framework;

namespace Alex.Rendering.Camera
{
    public class ThirdPersonCamera : Camera
    {
		public ThirdPersonCamera(int renderDistance, Vector3 positiuon, Vector3 rotation) : base(renderDistance)
		{
			Position = positiuon;
			Rotation = rotation;
		}


		private Vector3 _thirdPersonOffset =  new Vector3(0, 2.5f, 2.5f);
	    protected override void UpdateLookAt()
	    {
			Matrix rotationMatrix = Matrix.CreateRotationX(Rotation.X) *
			                        Matrix.CreateRotationY(Rotation.Y);

		    Vector3 lookAtOffset = Vector3.Transform(_thirdPersonOffset, rotationMatrix);

		    Target = Position;

		    Direction = Vector3.Transform(Vector3.Forward, rotationMatrix);


		    var heightOffset = new Vector3(0, 1.8f, 0);
			ViewMatrix = Matrix.CreateLookAt(Position + lookAtOffset, Target + heightOffset, Vector3.Up);
		}
    }
}
