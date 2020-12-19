using Alex.API.Entities;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
	public interface ICamera
	{
		BoundingFrustum BoundingFrustum { get; }
		float FarDistance { get; set; }
		float NearDistance { get; set; }
		float FOV { get; set; }
		float AspectRatio { get;  }
		Vector3 Position { get; set; }
		Matrix ProjectionMatrix { get; set; }
		Vector3 Rotation { get; set; }
		Vector3 Target { get; }
		Vector3 Forward
		{
			get => Vector3.Normalize(Target - Position);
		}
		Vector3 Up { get; set; }
		Matrix ViewMatrix { get; set; }

		void Move(Vector3 scale);
		void MoveTo(Vector3 position, Vector3 rotation);
		Vector3 PreviewMove(Vector3 amount);
		void Update(IUpdateArgs args);
	//	void Update(IUpdateArgs args, PlayerLocation entityLocation);
		void UpdateAspectRatio(float aspectRatio);
		void UpdateProjectionMatrix();

		void SetRenderDistance(int renderDistance);
	}

	public interface ICameraWrapper
	{
		void PreDraw(ICamera camera);
		Matrix View { get; }
		Matrix Projection { get; }
		Vector3 Position { get; }
		Vector3 Forward { get; }
		Vector3 Up { get; }
	}
}
