using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Alex.GuiDebugger.Models;

namespace Alex.GuiDebugger.Markup
{
	public class PropertyGridEditorDataTemplateSelector : DataTemplateSelector
	{

		private readonly Dictionary<Type, DataTemplate> _templates;


		public DataTemplate StringTemplate { get; set; }
		public DataTemplate IntTemplate { get; set; }
		public DataTemplate DoubleTemplate { get; set; }
		public DataTemplate EnumTemplate { get; set; }
		public DataTemplate BooleanTemplate { get; set; }



		public PropertyGridEditorDataTemplateSelector()
		{
			_templates = new Dictionary<Type, DataTemplate>();
		}

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null) return base.SelectTemplate(item, container);

			var type = item.GetType();
			if (_templates.TryGetValue(type, out var template))
			{
				return template;
			}

			return base.SelectTemplate(item, container);
		}
	}
}
