using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Alex.Gui.Common;
using Alex.Gui.Enums;
using Alex.Gui.Rendering;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Themes
{
	public struct UiElementStyle
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(UiElementStyle));
		
		public int Priority { get; set; }
		
		public UiElementStyleProperty<Color> BackgroundColor { get; set; }

		public UiElementStyleProperty<NinePatchTexture> Background { get; set; }

		public UiElementStyleProperty<Color> TextColor { get; set; }

		public UiElementStyleProperty<SpriteFont> TextFont { get; set; }

		//public int? Width { get; set; }

		//public int? Height { get; set; }

		//public Thickness Padding { get; set; }

		//public Thickness Margin { get; set; }

		//public HorizontalAlignment HorizontalAlignment { get; set; }

		//public VerticalAlignment VerticalAlignment { get; set; }

		public void ApplyStyle(UiElementStyle style)
		{
			//Log.InfoFormat("Apply Styles");
			var type = this.GetType();
			foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (typeof(IUiElementStyleProperty).IsAssignableFrom(prop.PropertyType))
				{
					var thisValue = (IUiElementStyleProperty) prop.GetValue(this);
					var styleValue = (IUiElementStyleProperty) prop.GetValue(style);
					
					if (thisValue == null || styleValue.Priority > thisValue.Priority)
					{
						prop.SetValue(this, styleValue);
					}
					else if (!thisValue.HasValue && styleValue.HasValue && thisValue.Priority >= styleValue.Priority)
					{
						prop.SetValue(this, styleValue);
					}

					//Log.InfoFormat("Found {1} {0}", prop.Name, prop.ReflectedType.Name);
				}
			}
		}
	}
}
