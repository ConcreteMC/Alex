using Microsoft.Xna.Framework;

namespace Alex.Common.Graphics
{
	public interface ICamera
	{
		BoundingFrustum BoundingFrustum  { get; }
		float           FarDistance      { get; set; }
		float           FieldOfView              { get; set; }
		float           AspectRatio      { get;  }
		Vector3         Position         { get; set; }
		Matrix        ProjectionMatrix { get; set; }
		Vector3         Rotation         { get; set; }
		Vector3         Target           { get; }
		Matrix        ViewMatrix       { get; set; }
		Vector3 Direction { get; }
		void MoveTo(Vector3 position, Vector3 rotation);
		void Update(IUpdateArgs args);
		void UpdateAspectRatio(float aspectRatio);
		void UpdateProjectionMatrix();

		void SetRenderDistance(int renderDistance);
	}
}
