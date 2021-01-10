using Alex.Api;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
	public interface ICamera
	{
		BoundingFrustum BoundingFrustum  { get; }
		float           FarDistance      { get; set; }
		float           FOV              { get; set; }
		float           AspectRatio      { get;  }
		Vector3         Position         { get; set; }
		MCMatrix        ProjectionMatrix { get; set; }
		Vector3         Rotation         { get; set; }
		Vector3         Target           { get; }
		MCMatrix        ViewMatrix       { get; set; }
		
		void MoveTo(Vector3 position, Vector3 rotation);
		void Update(IUpdateArgs args);
		void UpdateAspectRatio(float aspectRatio);
		void UpdateProjectionMatrix();

		void SetRenderDistance(int renderDistance);
	}
}
