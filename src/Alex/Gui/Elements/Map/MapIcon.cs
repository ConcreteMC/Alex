using System;
using Alex.Common.Gui.Graphics;
using Alex.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements.Map
{
	public class MapIcon
	{
		public int DrawOrder { get; set; } = 0;

		public virtual Vector3 Position { get; set; }
		public virtual float Rotation { get; set; } = 0f;
		
		public MapMarker Marker { get; set; }
		public Color Color { get; set; } = Color.White;
		public bool AlwaysShown { get; set; } = false;

		public MapIcon(MapMarker marker)
		{
			Marker = marker;
		}
	}
}