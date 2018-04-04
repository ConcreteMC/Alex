using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Entities;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
	public interface ICamera
	{
		BoundingFrustum BoundingFrustum { get; }
		float FarDistance { get; set; }
		Vector3 Position { get; set; }
		Matrix ProjectionMatrix { get; set; }
		Vector3 Rotation { get; set; }
		Vector3 Target { get; }
		Matrix ViewMatrix { get; set; }

		void Move(Vector3 scale);
		void MoveTo(Vector3 position, Vector3 rotation);
		Vector3 PreviewMove(Vector3 amount);
		void Update(IUpdateArgs args, IEntity entity);
		void UpdateAspectRatio(float aspectRatio);
	}
}
