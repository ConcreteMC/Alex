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
		
		private Vector3 _position;
		public Vector3 Position
		{
			get => _position;
			set
			{
				_position = value;
			}
		}

		public MapMarker Marker
		{
			get => _marker;
			set
			{
				_marker = value;
				//MarkerChanged(value);
			}
		}

		public float Rotation { get; set; } = 0f;

		public MapIcon(MapMarker marker)
		{
			//MiniMap = map;
			
		//	Anchor = Alignment.Fixed;
		//	base.RotationOrigin = new Vector2(4, 4);
                
		//	ResizeToImageSize = true;
			Marker = marker;
		}

		//public GuiTexture2D Value { get; private set; }
		private MapMarker _marker;

		//public virtual void 
	}
}