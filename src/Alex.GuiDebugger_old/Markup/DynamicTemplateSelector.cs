using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Alex.GuiDebugger.Markup
{
	/// <summary>
	/// Provides a means to specify DataTemplates to be selected from within WPF code
	/// </summary>
	public class DynamicTemplateSelector : DataTemplateSelector
	{
		/// <summary>
		/// Generic attached property specifying <see cref="Template"/>s
		/// used by the <see cref="DynamicTemplateSelector"/>
		/// </summary>
		/// <remarks>
		/// This attached property will allow you to set the templates you wish to be available whenever
		/// a control's TemplateSelector is set to an instance of <see cref="DynamicTemplateSelector"/>
		/// </remarks>
		public static readonly DependencyProperty TemplatesProperty =
			DependencyProperty.RegisterAttached("Templates", typeof(TemplateCollection), typeof(DataTemplateSelector),
												new FrameworkPropertyMetadata(new TemplateCollection(),
																			  FrameworkPropertyMetadataOptions
																				  .Inherits));


		/// <summary>
		/// Gets the value of the <paramref name="element"/>'s attached <see cref="TemplatesProperty"/>
		/// </summary>
		/// <param name="element">The <see cref="UIElement"/> who's attached template's property you wish to retrieve</param>
		/// <returns>The templates used by the givem <paramref name="element"/>
		/// when using the <see cref="DynamicTemplateSelector"/></returns>
		public static TemplateCollection GetTemplates(UIElement element)
		{
			return (TemplateCollection) element.GetValue(TemplatesProperty);
		}

		/// <summary>
		/// Sets the value of the <paramref name="element"/>'s attached <see cref="TemplatesProperty"/>
		/// </summary>
		/// <param name="element">The element to set the property on</param>
		/// <param name="collection">The collection of <see cref="Template"/>s to apply to this element</param>
		public static void SetTemplates(UIElement element, TemplateCollection collection)
		{
			element.SetValue(TemplatesProperty, collection);
		}

		/// <summary>
		/// Overriden base method to allow the selection of the correct DataTemplate
		/// </summary>
		/// <param name="item">The item for which the template should be retrieved</param>
		/// <param name="container">The object containing the current item</param>
		/// <returns>The <see cref="DataTemplate"/> to use when rendering the <paramref name="item"/></returns>
		public override System.Windows.DataTemplate SelectTemplate(object                          item,
																   System.Windows.DependencyObject container)
		{
			//This should ensure that the item we are getting is in fact capable of holding our property
			//before we attempt to retrieve it.
			if (!(container is UIElement))
				return base.SelectTemplate(item, container);

			//First, we gather all the templates associated with the current control through our dependency property
			TemplateCollection templates = GetTemplates(container as UIElement);
			if (templates == null || templates.Count == 0)
				base.SelectTemplate(item, container);

			//Then we go through them checking if any of them match our criteria
			foreach (var template in templates)
				//In this case, we are checking whether the type of the item
				//is the same as the type supported by our DataTemplate
				if (template.Value.IsInstanceOfType(item))
					//And if it is, then we return that DataTemplate
					return template.DataTemplate;

			//If all else fails, then we go back to using the default DataTemplate
			return base.SelectTemplate(item, container);
		}
	}

	/// <summary>
	/// Holds a collection of <see cref="Template"/> items
	/// for application as a control's DataTemplate.
	/// </summary>
	public class TemplateCollection : List<Template>
	{
	}

	/// <summary>
	/// Provides a link between a value and a <see cref="DataTemplate"/>
	/// for the <see cref="DynamicTemplateSelector"/>
	/// </summary>
	/// <remarks>
	/// In this case, our value is a <see cref="System.Type"/> which we are attempting to match
	/// to a <see cref="DataTemplate"/>
	/// </remarks>
	public class Template : DependencyObject
	{
		/// <summary>
		/// Provides the value used to match this <see cref="DataTemplate"/> to an item
		/// </summary>
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(Type), typeof(Template));

		/// <summary>
		/// Provides the <see cref="DataTemplate"/> used to render items matching the <see cref="Value"/>
		/// </summary>
		public static readonly DependencyProperty DataTemplateProperty =
			DependencyProperty.Register("DataTemplate", typeof(DataTemplate), typeof(Template));

		/// <summary>
		/// Gets or Sets the value used to match this <see cref="DataTemplate"/> to an item
		/// </summary>
		public Type Value
		{
			get { return (Type) GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		/// <summary>
		/// Gets or Sets the <see cref="DataTemplate"/> used to render items matching the <see cref="Value"/>
		/// </summary>
		public DataTemplate DataTemplate
		{
			get { return (DataTemplate) GetValue(DataTemplateProperty); }
			set { SetValue(DataTemplateProperty, value); }
		}
	}
}