using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		
		public ObservableCollection<PropertyGridItem> PropertyItems { get; set; }

		public PropertyGrid()
		{
			PropertyItems = new ObservableCollection<PropertyGridItem>();
			InitializeComponent();
			DataContext = this;
		}

		
		private static void OnItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// Reload property items
			((PropertyGrid) d).LoadPropertyItems(e.NewValue);
		}

		private void LoadPropertyItems(object obj)
		{
			if (obj == null)
			{
				PropertyItems.Clear();
				return;
			}



		}
	}
}
