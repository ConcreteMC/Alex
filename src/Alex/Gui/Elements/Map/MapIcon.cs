using System;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Elements.Map
{
	public class MapIcon : IMapElement
	{
		public int DrawOrder { get; set; } = 0;

		public virtual Vector3 Position { get; set; }

		public virtual float Rotation { get; set; } = 0f;

		public MapMarker Marker { get; set; }
		public Color Color { get; set; } = Color.White;
		public bool AlwaysShown { get; set; } = false;

		public string Label { get; set; } = null;

		public MapIcon(MapMarker marker)
		{
			Marker = marker;
		}

		/// <inheritdoc />
		public Texture2D GetTexture(GraphicsDevice device)
		{
			throw new NotImplementedException();
		}
	}

	public class UserMapIcon : MapIcon
	{
		/// <inheritdoc />
		public UserMapIcon(MapMarker marker) : base(marker)
		{
			AlwaysShown = true;
		}
	}
}