using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Models;

namespace Alex.GuiDebugger.Controls
{
	/// <summary>
	/// Interaction logic for PropertyGrid.xaml
	/// </summary>
	public partial class PropertyGrid : UserControl
	{
		#region Item

		public static readonly DependencyProperty ItemProperty =
			DependencyProperty.Register(nameof(Item), typeof(object), typeof(PropertyGrid), new PropertyMetadata(default(object), OnItemPropertyChanged));


		public object Item
		{
			get { return (object) GetValue(ItemProperty); }
			set { SetValue(ItemProperty, value); }
		}

		#endregion

		#region PropertyItems

		public static readonly DependencyProperty PropertyItemsProperty =
			DependencyProperty.Register(nameof(PropertyItems), typeof(ObservableCollection<PropertyGridItem>), typeof(PropertyGrid), new PropertyMetadata(default(ObservableCollection<PropertyGridItem>)));

		public ObservableCollection<PropertyGridItem> PropertyItems
		{
			get { return (ObservableCollection<PropertyGridItem>) GetValue(PropertyItemsProperty); }
			set { SetValue(PropertyItemsProperty, value); }
		}

		#endregion
		
		public PropertyGrid()
		{
			PropertyItems = new ObservableCollection<PropertyGridItem>();
			InitializeComponent();
			DataContext = this;

			if (DesignerProperties.GetIsInDesignMode(this))
			{
				//PropertyItems.Add(new PropertyGridItem()
				//{
				//	Name = "Id",
				//	Value = Guid.NewGuid()
				//});
				//PropertyItems.Add(new PropertyGridItem()
				//{
				//	Name = "X",
				//	Value = 0
				//});
				//PropertyItems.Add(new PropertyGridItem()
				//{
				//	Name  = "Y",
				//	Value = 0
				//});
				//PropertyItems.Add(new PropertyGridItem()
				//{
				//	Name  = "Position",
				//	Value = "0, 0"
				//});
			}
		}

		
		private static void OnItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// Reload property items
			//((PropertyGrid) d).LoadPropertyItems(e.NewValue);
		}

		//private void LoadPropertyItems(object obj)
		//{
		//	if (obj == null)
		//	{
		//		PropertyItems.Clear();
		//		return;
		//	}

		//	if (obj is GuiElementInfo guiElementInfo)
		//	{
		//		PropertyItems.Clear();
		//		foreach (var propInfo in guiElementInfo.PropertyInfos)
		//		{
		//			PropertyItems.Add(new PropertyGridItem()
		//			{
		//				Name = propInfo.Name,
		//				Value = propInfo.StringValue
		//			});
		//		}
		//	}
		//}
	}
}
