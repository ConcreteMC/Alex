using System;
using Alex.API.Graphics;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

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
		bool IsCollidingWithWorld { get; set; }
		
		long Age { get; set; }
		float Scale { get; set; }

		BoundingBox GetBoundingBox();

		void EntityHurt();
		
		long RenderedVertices { get; }
		void Render(IRenderArgs renderArgs);
		void Update(IUpdateArgs args);
		void UpdateHeadYaw(float headYaw);

		void RenderNametag(IRenderArgs renderArgs);
		
		float PositionOffset { get; set; }

		//void HandleMetadata(MetadataDictionary metadata);
		
		bool IsColliding(IEntity other);

		bool IsColliding(BoundingBox bbox, IEntity other);
		double DistanceToHorizontal(IEntity entity);
		double DistanceTo(IEntity entity);

		bool CanSee(IEntity target);
	}
}
