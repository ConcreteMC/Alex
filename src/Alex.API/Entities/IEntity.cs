using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Entities
{
	public interface IEntity
	{
		UUID UUID { get; set; }
		long EntityId { get; set; }
		bool IsSpawned { get; set; }
		PlayerLocation KnownPosition { get; set; }
		Vector3 Velocity { get; set; }
		string NameTag { get; set; }

		bool NoAi { get; set; }
		bool HideNameTag { get; set; }
		bool Silent { get; set; }
		bool IsInWater { get; set; }
		bool IsOutOfWater { get; }

		long Age { get; set; }
		double Scale { get; set; }
		double Height { get; set; }
		double Width { get; set; }
		double Length { get; set; }
		double Drag { get; set; }
		double Gravity { get; set; }

		BoundingBox GetBoundingBox();

		void Render(IRenderArgs renderArgs);
		void Update(IUpdateArgs args);

		void RenderNametag(IRenderArgs renderArgs);
	}
}
