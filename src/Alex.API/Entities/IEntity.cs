using System;
using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Entities
{
	public interface IEntity : IPhysicsEntity, IDisposable
	{
		UUID UUID { get; set; }
		long EntityId { get; set; }
		bool IsSpawned { get; set; }
		PlayerLocation KnownPosition { get; set; }
		string NameTag { get; set; }

		bool NoAi { get; set; }
		bool HideNameTag { get; set; }
		bool Silent { get; set; }
		bool IsInWater { get; set; }
		bool IsOutOfWater { get; }

		long Age { get; set; }
		double Scale { get; set; }

		BoundingBox GetBoundingBox();

		void Render(IRenderArgs renderArgs);
		void Update(IUpdateArgs args);
		void UpdateHeadYaw(float headYaw);

		void RenderNametag(IRenderArgs renderArgs);
	}
}
