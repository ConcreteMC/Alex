using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Gui.Rendering
{
	public class UiSkin
	{

		private Dictionary<string, UiElementStyle> Styles { get; }

		public UiSkin()
		{
			Styles = new Dictionary<string, UiElementStyle>();
		}

		public UiElementStyle GetStyle(string name)
		{
			if (!Styles.TryGetValue(name.ToLowerInvariant(), out var style))
			{
				style = new UiElementStyle();
			}
			return style;
		}

		public void DefineStyle<TElement>(UiElementStyle style) where TElement : UiElement
		{
			Styles.Add(typeof(TElement).FullName.ToLowerInvariant(), style);
		}

		public void ApplyStyle<TElement>(TElement element) where TElement : UiElement
		{
			var style = GetStyle(typeof(TElement).FullName);
			element.ApplyStyle(style);
		}


	}
}
