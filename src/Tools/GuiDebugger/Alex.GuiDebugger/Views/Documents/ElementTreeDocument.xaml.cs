using System;
using System.Collections.Specialized;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Alex.GuiDebugger.Views.Documents
{
	public class ElementTreeDocument : UserControl
	{
		public ElementTreeDocument()
		{
			this.InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnDataContextChanged(EventArgs e)
		{
			base.OnDataContextChanged(e);

			//UpdateView();

			//if (DataContext is ViewModels.Documents.ElementTreeDocument vm)
			//{
			//	vm.Properties.CollectionChanged += PropertiesOnCollectionChanged;
			//}
		}

		//private void PropertiesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		//{
		//	UpdateView();
		//}

		//private void UpdateView()
		//{
		//	if (DataContext is ViewModels.Documents.ElementTreeDocument vm)
		//	{
		//		var dg = this.FindControl<DataGrid>("PropertiesDataGrid");
		//		dg.IsReadOnly = true;

		//		var collection = new DataGridCollectionView(vm.Properties);
		//		dg.Items = collection;
		//	}
		//}
	}
}