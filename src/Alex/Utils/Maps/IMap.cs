using System;
using System.Collections.Generic;
using Alex.Common.Utils.Vectors;
using Alex.Gui.Elements.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Utils
{
	public interface IMap : IDisposable
	{
		int Width { get; }
		int Height { get; }
		float Scale { get; }
		Vector3 Center { get; }
		float Rotation { get; }

		uint[] GetData();

		Texture2D GetTexture(GraphicsDevice device);

		void Add(MapIcon icon);

		void Remove(MapIcon icon);

		IEnumerable<MapIcon> GetMarkers(ChunkCoordinates center, int radius);

		IEnumerable<IMapElement> GetSections(ChunkCoordinates center, int radius);
	}
}