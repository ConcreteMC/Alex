using Alex.API.Entities;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
	public interface ICamera
	{
		BoundingFrustum BoundingFrustum { get; }
		float FarDistance { get; set; }
		float FOV { get; set; }
		Vector3 Position { get; set; }
		Matrix ProjectionMatrix { get; set; }
		Vector3 Rotation { get; set; }
		Vector3 Target { get; }
		Matrix ViewMatrix { get; set; }

		void Move(Vector3 scale);
		void MoveTo(Vector3 position, Vector3 rotation);
		Vector3 PreviewMove(Vector3 amount);
		void Update(IUpdateArgs args, IEntity entity);
		void Update(IUpdateArgs args, PlayerLocation entityLocation);
		void UpdateAspectRatio(float aspectRatio);
		void UpdateProjectionMatrix();
	}
}
